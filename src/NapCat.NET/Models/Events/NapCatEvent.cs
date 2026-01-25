using System.Text.Json;
using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Events;

public abstract class NapCatEvent
{
    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("self_id")]
    public long SelfId { get; set; }

    [JsonPropertyName("post_type")]
    public string PostType { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class UnknownEvent : NapCatEvent
{
    public JsonElement Raw { get; init; }
}
