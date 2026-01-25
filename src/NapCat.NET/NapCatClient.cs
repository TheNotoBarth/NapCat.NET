using System.Text.Json;
using NapCat.NET.Interfaces;
using NapCat.NET.Models.Events;
using NapCat.NET.Models.Requests;
using NapCat.NET.Models.Responses;

namespace NapCat.NET;

public class NapCatClient
{
    public INapCatTransport Transport { get; }
    public NapCatApi Api { get; }

    public event EventHandler<NapCatEvent>? EventReceived;
    public event EventHandler<MessageEventBase>? MessageReceived;
    public event EventHandler<MessageSentEventBase>? MessageSentReceived;
    public event EventHandler<NoticeEventBase>? NoticeReceived;
    public event EventHandler<RequestEventBase>? RequestReceived;
    public event EventHandler<MetaEventBase>? MetaEventReceived;
    public event EventHandler<BaseResponse<System.Text.Json.JsonElement>>? ResponseReceived;
    public event EventHandler? Disconnected;

    public NapCatClient(INapCatTransport transport)
    {
        Transport = transport;
        Api = new NapCatApi(transport);

        Transport.EventReceived += (_, evt) => HandleEvent(evt);
        Transport.ResponseReceived += (_, resp) => ResponseReceived?.Invoke(this, resp);
        Transport.OnDisconnected += (_, _) => Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public Task ConnectAsync(CancellationToken token = default) => Transport.ConnectAsync(token);

    public Task DisconnectAsync() => Transport.DisconnectAsync();

    public Task<BaseResponse<SendGroupMsgResponse>?> SendGroupMsgAsync(long groupId, List<Core.MessageSegment> message, CancellationToken token = default)
        => Api.SendGroupMsgAsync(groupId, message, token);

    public Task<BaseResponse<GetGroupInfoResponse>?> GetGroupInfoAsync(long groupId, CancellationToken token = default)
        => Api.GetGroupInfoAsync(groupId, token);

    public Task<BaseResponse<TResponse>?> SendRequestAsync<TParams, TResponse>(RequestBase<TParams> request, CancellationToken token = default)
        => Transport.SendRequestAsync<TParams, TResponse>(request, token);

    public Task<BaseResponse<JsonElement>?> SendRawAsync(string action, object? parameters = null, CancellationToken token = default)
        => Api.SendRawAsync(action, parameters, token);

    private void HandleEvent(NapCatEvent evt)
    {
        EventReceived?.Invoke(this, evt);

        switch (evt)
        {
            case MessageSentEventBase messageSent:
                MessageSentReceived?.Invoke(this, messageSent);
                break;
            case MessageEventBase message:
                MessageReceived?.Invoke(this, message);
                break;
            case NoticeEventBase notice:
                NoticeReceived?.Invoke(this, notice);
                break;
            case RequestEventBase request:
                RequestReceived?.Invoke(this, request);
                break;
            case MetaEventBase meta:
                MetaEventReceived?.Invoke(this, meta);
                break;
        }
    }
}
