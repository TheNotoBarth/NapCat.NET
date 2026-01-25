using System.Text.Json;
using NapCat.NET.Models.Events;
using NapCat.NET.Models.Requests;
using NapCat.NET.Models.Responses;

namespace NapCat.NET.Interfaces;

/// <summary>
/// 网络客户端接口
/// </summary>
public interface INapCatTransport : IDisposable
{
    /// <summary>
    /// 连接名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 当前是否已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 启动连接
    /// </summary>
    Task ConnectAsync(CancellationToken token = default);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 发送消息（原始 JSON 字符串）
    /// </summary>
    /// <param name="message">JSON 字符串</param>
    Task SendAsync(string message);

    /// <summary>
    /// 发送请求并等待强类型响应
    /// </summary>
    Task<BaseResponse<TResponse>?> SendRequestAsync<TParams, TResponse>(
        RequestBase<TParams> request,
        CancellationToken token = default);

    /// <summary>
    /// 收到响应消息
    /// </summary>
    event EventHandler<BaseResponse<JsonElement>> ResponseReceived;

    /// <summary>
    /// 收到上报事件
    /// </summary>
    event EventHandler<NapCatEvent> EventReceived;

    /// <summary>
    /// 连接断开
    /// </summary>
    event EventHandler OnDisconnected;
}