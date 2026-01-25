using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Responses;

/// <summary>
/// 发送群消息响应数据结构
/// </summary>
public class SendGroupMsgResponse
{
    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("message_id")]
    public long MessageId { get; set; }
}