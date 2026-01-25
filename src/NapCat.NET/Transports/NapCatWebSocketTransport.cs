using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NapCat.NET.Interfaces;
using NapCat.NET.Models.Events;
using NapCat.NET.Models.Requests;
using NapCat.NET.Models.Responses;
using NapCat.NET.Serialization;

namespace NapCat.NET.Transports;

public class NapCatWebSocketTransport : INapCatTransport
{
    private readonly ILogger _logger;
    private readonly Uri _targetUri;
    private ClientWebSocket? _client;
    private CancellationTokenSource? _receiveCts;
    private Task? _receiveLoop;
    private bool _isDisposed;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingRequests = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
    private readonly JsonSerializerOptions _eventOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new NapCatEventJsonConverter(), new MessageSegmentJsonConverter() }
    };
    private readonly TimeSpan _requestTimeout;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public string Name => "NapCat-Forward-WS";
    public bool IsConnected => _client?.State == WebSocketState.Open;

    public event EventHandler<BaseResponse<JsonElement>>? ResponseReceived;
    public event EventHandler<NapCatEvent>? EventReceived;
    public event EventHandler? OnDisconnected;

    public NapCatWebSocketTransport(string url, ILogger logger, TimeSpan? requestTimeout = null)
    {
        _targetUri = new Uri(url);
        _logger = logger;
        _requestTimeout = requestTimeout ?? TimeSpan.FromSeconds(60);
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        if (_client == null)
        {
            return;
        }

        var buffer = new byte[8192];
        using var ms = new MemoryStream();

        try
        {
            while (!token.IsCancellationRequested && _client.State == WebSocketState.Open)
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    ms.Write(buffer, 0, result.Count);
                    if (result.EndOfMessage)
                    {
                        var text = Encoding.UTF8.GetString(ms.ToArray());
                        ms.SetLength(0);

                        if (_logger.IsEnabled(LogLevel.Trace))
                        {
                            _logger.LogTrace("[{Name}] 收到消息: {Msg}", Name, text);
                        }

                        HandleIncomingText(text);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[{Name}] 接收循环异常", Name);
        }
        finally
        {
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HandleIncomingText(string text)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            if (root.TryGetProperty("post_type", out _))
            {
                var evt = JsonSerializer.Deserialize<NapCatEvent>(text, _eventOptions);
                if (evt != null)
                {
                    EventReceived?.Invoke(this, evt);
                }
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[{Name}] 无法解析事件消息，忽略: {Text}", Name, text);
        }

        BaseResponse<JsonElement>? envelope = null;
        string? echo = null;

        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.TryGetProperty("echo", out var echoElement))
            {
                echo = echoElement.GetString();
            }

            try
            {
                envelope = JsonSerializer.Deserialize<BaseResponse<JsonElement>>(text, _jsonOptions);
            }
            catch
            {
                envelope = new BaseResponse<JsonElement>
                {
                    Echo = echo,
                    Data = root.Clone()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[{Name}] 无法解析响应消息，忽略: {Text}", Name, text);
            return;
        }

        if (envelope == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(envelope.Echo))
        {
            envelope.Echo = echo;
        }

        if (!string.IsNullOrWhiteSpace(envelope.Echo) && _pendingRequests.TryRemove(envelope.Echo, out var tcs))
        {
            tcs.TrySetResult(text);
        }

        try
        {
            ResponseReceived?.Invoke(this, envelope);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[{Name}] ResponseReceived 处理异常", Name);
        }
    }

    public async Task ConnectAsync(CancellationToken token = default)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(NapCatWebSocketTransport));

        if (_client != null && _client.State == WebSocketState.Open)
        {
            return;
        }

        _logger.LogInformation("[{Name}] 正在连接到 {Uri} ...", Name, _targetUri);

        _client = new ClientWebSocket();
        _client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        await _client.ConnectAsync(_targetUri, token);

        _receiveCts = new CancellationTokenSource();
        _receiveLoop = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token));
        _logger.LogInformation("[{Name}] 连接已建立", Name);
    }

    public async Task DisconnectAsync()
    {
        if (_client == null)
        {
            return;
        }

        _logger.LogInformation("[{Name}] 正在主动断开连接...", Name);
        _receiveCts?.Cancel();
        if (_client.State == WebSocketState.Open || _client.State == WebSocketState.CloseReceived)
        {
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
        }

        if (_receiveLoop != null)
        {
            await _receiveLoop;
        }
    }

    public async Task SendAsync(string message)
    {
        if (!IsConnected)
        {
            _logger.LogError("[{Name}] 发送失败，NapCat未连接", Name);
            return;
        }

        _logger.LogDebug("[{Name}] 发送消息: {Msg}", Name, message);

        var buffer = Encoding.UTF8.GetBytes(message);
        await _sendLock.WaitAsync();
        try
        {
            if (_client == null)
            {
                return;
            }
            await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task<BaseResponse<TResponse>?> SendRequestAsync<TParams, TResponse>(
        RequestBase<TParams> request,
        CancellationToken token = default)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(NapCatWebSocketTransport));
        if (request == null) throw new ArgumentNullException(nameof(request));

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pendingRequests.TryAdd(request.Echo, tcs))
        {
            throw new InvalidOperationException($"重复的 echo: {request.Echo}");
        }

        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            await SendAsync(json);

            using var timeoutCts = new CancellationTokenSource(_requestTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, linkedCts.Token));
            if (completed != tcs.Task)
            {
                _pendingRequests.TryRemove(request.Echo, out _);
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                throw new TimeoutException($"等待响应超时 (echo={request.Echo})");
            }
            var responseJson = await tcs.Task;
            return JsonSerializer.Deserialize<BaseResponse<TResponse>>(responseJson, _jsonOptions);
        }
        finally
        {
            _pendingRequests.TryRemove(request.Echo, out _);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _receiveCts?.Cancel();
        _client?.Dispose();
        _isDisposed = true;
        _logger.LogInformation("[{Name}] 资源已释放", Name);
    }
}
