using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NapCat.NET.Models.Events;

namespace NapCat.NET.Serialization;

public sealed class NapCatEventJsonConverter : JsonConverter<NapCatEvent>
{
    public override NapCatEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("post_type", out var postTypeElement))
        {
            return new UnknownEvent { Raw = root.Clone() };
        }

        var postType = postTypeElement.GetString();
        if (string.IsNullOrWhiteSpace(postType))
        {
            return new UnknownEvent { Raw = root.Clone() };
        }

        var json = root.GetRawText();

        return postType switch
        {
            "message" => DeserializeMessage(json, root, options),
            "message_sent" => DeserializeMessageSent(json, root, options),
            "notice" => DeserializeNotice(json, root, options),
            "request" => DeserializeRequest(json, root, options),
            "meta_event" => DeserializeMeta(json, root, options),
            _ => new UnknownEvent { Raw = root.Clone() }
        };
    }

    public override void Write(Utf8JsonWriter writer, NapCatEvent value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }

    private static NapCatEvent DeserializeMessage(string json, JsonElement root, JsonSerializerOptions options)
    {
        if (root.TryGetProperty("message_type", out var messageTypeElement))
        {
            var messageType = messageTypeElement.GetString();
            if (string.Equals(messageType, "group", StringComparison.OrdinalIgnoreCase))
            {
                return DeserializeOrUnknown<GroupMessageEvent>(json, root, options);
            }

            if (string.Equals(messageType, "private", StringComparison.OrdinalIgnoreCase))
            {
                return DeserializeOrUnknown<PrivateMessageEvent>(json, root, options);
            }
        }

        return new UnknownEvent { Raw = root.Clone() };
    }

    private static NapCatEvent DeserializeMessageSent(string json, JsonElement root, JsonSerializerOptions options)
    {
        if (root.TryGetProperty("message_type", out var messageTypeElement))
        {
            var messageType = messageTypeElement.GetString();
            if (string.Equals(messageType, "group", StringComparison.OrdinalIgnoreCase))
            {
                return DeserializeOrUnknown<GroupMessageSentEvent>(json, root, options);
            }

            if (string.Equals(messageType, "private", StringComparison.OrdinalIgnoreCase))
            {
                return DeserializeOrUnknown<PrivateMessageSentEvent>(json, root, options);
            }
        }

        return new UnknownEvent { Raw = root.Clone() };
    }

    private static NapCatEvent DeserializeNotice(string json, JsonElement root, JsonSerializerOptions options)
    {
        if (root.TryGetProperty("notice_type", out var noticeTypeElement))
        {
            var noticeType = noticeTypeElement.GetString();
            var subType = root.TryGetProperty("sub_type", out var subTypeElement) ? subTypeElement.GetString() : null;

            return noticeType switch
            {
                "friend_recall" => DeserializeOrUnknown<FriendRecallNoticeEvent>(json, root, options),
                "group_admin" => DeserializeOrUnknown<GroupAdminNoticeEvent>(json, root, options),
                "group_ban" => DeserializeOrUnknown<GroupBanNoticeEvent>(json, root, options),
                "group_increase" => DeserializeOrUnknown<GroupIncreaseNoticeEvent>(json, root, options),
                "group_upload" => DeserializeOrUnknown<GroupUploadNoticeEvent>(json, root, options),
                "group_msg_emoji_like" => DeserializeOrUnknown<GroupMsgEmojiLikeNoticeEvent>(json, root, options),
                "notify" => DeserializeNotifyNotice(json, subType, options, root),
                _ => new UnknownEvent { Raw = root.Clone() }
            };
        }

        return new UnknownEvent { Raw = root.Clone() };
    }

    private static NapCatEvent DeserializeNotifyNotice(string json, string? subType, JsonSerializerOptions options, JsonElement root)
    {
        return subType switch
        {
            "poke" => DeserializeOrUnknown<NotifyPokeNoticeEvent>(json, root, options),
            "title" => DeserializeOrUnknown<NotifyTitleNoticeEvent>(json, root, options),
            "profile_like" => DeserializeOrUnknown<NotifyProfileLikeNoticeEvent>(json, root, options),
            "group_name" => DeserializeOrUnknown<GroupNameNoticeEvent>(json, root, options),
            _ => new UnknownEvent { Raw = root.Clone() }
        };
    }

    private static NapCatEvent DeserializeRequest(string json, JsonElement root, JsonSerializerOptions options)
    {
        if (root.TryGetProperty("request_type", out var requestTypeElement))
        {
            var requestType = requestTypeElement.GetString();
            var subType = root.TryGetProperty("sub_type", out var subTypeElement) ? subTypeElement.GetString() : null;

            if (string.Equals(requestType, "friend", StringComparison.OrdinalIgnoreCase))
            {
                return DeserializeOrUnknown<FriendRequestEvent>(json, root, options);
            }

            if (string.Equals(requestType, "group", StringComparison.OrdinalIgnoreCase))
            {
                return subType switch
                {
                    "add" => DeserializeOrUnknown<GroupAddRequestEvent>(json, root, options),
                    "invite" => DeserializeOrUnknown<GroupInviteRequestEvent>(json, root, options),
                    _ => new UnknownEvent { Raw = root.Clone() }
                };
            }
        }

        return new UnknownEvent { Raw = root.Clone() };
    }

    private static NapCatEvent DeserializeMeta(string json, JsonElement root, JsonSerializerOptions options)
    {
        if (root.TryGetProperty("meta_event_type", out var metaTypeElement))
        {
            var metaType = metaTypeElement.GetString();
            return metaType switch
            {
                "heartbeat" => DeserializeOrUnknown<HeartbeatMetaEvent>(json, root, options),
                "lifecycle" => DeserializeOrUnknown<LifecycleMetaEvent>(json, root, options),
                _ => new UnknownEvent { Raw = root.Clone() }
            };
        }

        return new UnknownEvent { Raw = root.Clone() };
    }

    private static NapCatEvent DeserializeOrUnknown<T>(string json, JsonElement root, JsonSerializerOptions options)
        where T : NapCatEvent
    {
        var result = JsonSerializer.Deserialize<T>(json, options);
        return result is null ? new UnknownEvent { Raw = root.Clone() } : result;
    }
}
