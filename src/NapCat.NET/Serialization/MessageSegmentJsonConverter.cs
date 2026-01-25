using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NapCat.NET.Core;

namespace NapCat.NET.Serialization;

public sealed class MessageSegmentJsonConverter : JsonConverter<MessageSegment>
{
    public override MessageSegment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var type = root.TryGetProperty("type", out var typeElement)
            ? typeElement.GetString() ?? string.Empty
            : string.Empty;

        Dictionary<string, object?>? data = null;
        if (root.TryGetProperty("data", out var dataElement))
        {
            if (dataElement.ValueKind == JsonValueKind.Object)
            {
                data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in dataElement.EnumerateObject())
                {
                    data[prop.Name] = ConvertElement(prop.Value);
                }
            }
            else if (dataElement.ValueKind != JsonValueKind.Undefined && dataElement.ValueKind != JsonValueKind.Null)
            {
                data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["value"] = ConvertElement(dataElement)
                };
            }
        }

        return new MessageSegment(type, data);
    }

    public override void Write(Utf8JsonWriter writer, MessageSegment value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        writer.WritePropertyName("data");
        JsonSerializer.Serialize(writer, value.Data, options);
        writer.WriteEndObject();
    }

    private static object? ConvertElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertObject(element),
            JsonValueKind.Array => ConvertArray(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private static Dictionary<string, object?> ConvertObject(JsonElement element)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = ConvertElement(prop.Value);
        }
        return dict;
    }

    private static List<object?> ConvertArray(JsonElement element)
    {
        var list = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            list.Add(ConvertElement(item));
        }
        return list;
    }
}
