using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Events;

public abstract class RequestEventBase : NapCatEvent
{
    [JsonPropertyName("request_type")]
    public string RequestType { get; set; } = string.Empty;

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }

    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("group_id")]
    public long? GroupId { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("flag")]
    public string? Flag { get; set; }
}

public sealed class FriendRequestEvent : RequestEventBase
{
}

public sealed class GroupAddRequestEvent : RequestEventBase
{
}

public sealed class GroupInviteRequestEvent : RequestEventBase
{
}
