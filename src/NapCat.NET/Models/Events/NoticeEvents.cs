using System.Text.Json;
using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Events;

public abstract class NoticeEventBase : NapCatEvent
{
    [JsonPropertyName("notice_type")]
    public string NoticeType { get; set; } = string.Empty;

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }

    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("group_id")]
    public long? GroupId { get; set; }
}

public sealed class FriendRecallNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("message_id")]
    public long MessageId { get; set; }
}

public sealed class GroupAdminNoticeEvent : NoticeEventBase
{
}

public sealed class GroupBanNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("operator_id")]
    public long OperatorId { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}

public sealed class GroupIncreaseNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("operator_id")]
    public long OperatorId { get; set; }
}

public sealed class GroupUploadNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("file")]
    public GroupUploadFileInfo? File { get; set; }
}

public sealed class GroupMsgEmojiLikeNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("message_id")]
    public long MessageId { get; set; }

    [JsonPropertyName("likes")]
    public List<EmojiLikeInfo>? Likes { get; set; }

    [JsonPropertyName("is_add")]
    public bool IsAdd { get; set; }
}

public sealed class NotifyPokeNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("target_id")]
    public long TargetId { get; set; }

    [JsonPropertyName("raw_info")]
    public JsonElement? RawInfo { get; set; }
}

public sealed class NotifyTitleNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public sealed class NotifyProfileLikeNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("operator_id")]
    public long OperatorId { get; set; }

    [JsonPropertyName("operator_nick")]
    public string? OperatorNick { get; set; }

    [JsonPropertyName("times")]
    public int Times { get; set; }
}

public sealed class GroupNameNoticeEvent : NoticeEventBase
{
    [JsonPropertyName("name_new")]
    public string? NameNew { get; set; }
}

public sealed class GroupUploadFileInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("busid")]
    public int BusId { get; set; }
}

public sealed class EmojiLikeInfo
{
    [JsonPropertyName("emoji_id")]
    public string? EmojiId { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
