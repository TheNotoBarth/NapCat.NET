using System.Text.Json.Serialization;
using NapCat.NET.Core;

namespace NapCat.NET.Models.Requests;

/// <summary>
/// 发送群消息请求数据结构
/// </summary>
public class SendGroupMsgRequest(long groupId, List<MessageSegment> message, string? echo = null)
    : RequestBase<SendGroupMsgParams>(ApiName, new SendGroupMsgParams(groupId, message), echo)
{
    public const string ApiName = "send_group_msg";
}

public class SendGroupMsgParams(long groupId, List<MessageSegment> message)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("message")]
    public List<MessageSegment> Message { get; } = message;
}
