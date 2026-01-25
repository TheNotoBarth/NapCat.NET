using System.Collections.Generic;
using NapCat.NET.Core;
using System.Text.Json;
using NapCat.NET.Interfaces;
using NapCat.NET.Models.Requests;
using NapCat.NET.Models.Responses;

namespace NapCat.NET;

public class NapCatApi
{
    private readonly INapCatTransport _transport;

    public NapCatApi(INapCatTransport transport)
    {
        _transport = transport;
    }

    public Task<BaseResponse<SendGroupMsgResponse>?> SendGroupMsgAsync(
        long groupId,
        List<MessageSegment> message,
        CancellationToken token = default)
    {
        var request = new SendGroupMsgRequest(groupId, message);
        return _transport.SendRequestAsync<SendGroupMsgParams, SendGroupMsgResponse>(request, token);
    }

    public Task<BaseResponse<GetGroupInfoResponse>?> GetGroupInfoAsync(
        long groupId,
        CancellationToken token = default)
    {
        var request = new GetGroupInfoRequest(groupId);
        return _transport.SendRequestAsync<GetGroupInfoParams, GetGroupInfoResponse>(request, token);
    }

    /// <summary>
    /// 调用未强类型化的 API
    /// </summary>
    public Task<BaseResponse<JsonElement>?> SendRawAsync(
        string action,
        object? parameters = null,
        CancellationToken token = default)
    {
        var request = new RawRequest(action, parameters);
        return _transport.SendRequestAsync<object?, JsonElement>(request, token);
    }
}