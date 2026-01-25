using System.Text.Json;
using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Events;

public abstract class MetaEventBase : NapCatEvent
{
    [JsonPropertyName("meta_event_type")]
    public string MetaEventType { get; set; } = string.Empty;

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }
}

public sealed class HeartbeatMetaEvent : MetaEventBase
{
    [JsonPropertyName("status")]
    public JsonElement? Status { get; set; }

    [JsonPropertyName("interval")]
    public long? Interval { get; set; }
}

public sealed class LifecycleMetaEvent : MetaEventBase
{
}
