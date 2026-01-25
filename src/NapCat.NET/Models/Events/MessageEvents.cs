using System.Text.Json.Serialization;
using NapCat.NET.Core;

namespace NapCat.NET.Models.Events;

public abstract class MessageEventBase : NapCatEvent
{
    [JsonPropertyName("message_type")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("message_id")]
    public long MessageId { get; set; }

    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; set; }

    [JsonPropertyName("real_id")]
    public long RealId { get; set; }

    [JsonPropertyName("real_seq")]
    public string? RealSeq { get; set; }

    [JsonPropertyName("raw_message")]
    public string? RawMessage { get; set; }

    [JsonPropertyName("font")]
    public int Font { get; set; }

    [JsonPropertyName("message")]
    public List<MessageSegment>? Message { get; set; }

    [JsonPropertyName("message_format")]
    public string? MessageFormat { get; set; }

    [JsonPropertyName("sender")]
    public SenderInfo? Sender { get; set; }

    [JsonPropertyName("target_id")]
    public long? TargetId { get; set; }
}

public abstract class MessageSentEventBase : MessageEventBase
{
    [JsonPropertyName("message_sent_type")]
    public string? MessageSentType { get; set; }
}

public sealed class PrivateMessageEvent : MessageEventBase
{
}

public sealed class GroupMessageEvent : MessageEventBase
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }
}

public sealed class PrivateMessageSentEvent : MessageSentEventBase
{
}

public sealed class GroupMessageSentEvent : MessageSentEventBase
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }
}

public sealed class SenderInfo
{
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("card")]
    public string? Card { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}
