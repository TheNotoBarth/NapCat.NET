using System.Text.Json.Serialization;

namespace NapCat.NET.Core;
public class MessageChain
{
    private readonly List<MessageSegment> _segments = new();

    public MessageChain() { }

    /// <summary>
    /// 添加纯文本消息
    /// </summary>
    public MessageChain Text(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            // 如果上一个是文本类型，则合并
            if (_segments.Count > 0 && _segments[^1].Type == "text")
            {
                var lastSegment = _segments[^1];
                if (lastSegment.Data.TryGetValue("text", out var existingTextObj) && existingTextObj is string existingText)
                {
                    lastSegment.Data["text"] = existingText + text;
                    return this;
                }
            }
            _segments.Add(new MessageSegment("text", new Dictionary<string, object?>
            {
                { "text", text }
            }));
        }
        return this;
    }

    /// <summary>
    /// @某人
    /// </summary>
    /// <param name="qq">QQ号，支持数字或字符串</param>
    public MessageChain At(object qq)
    {
        if (qq != null)
        {
            _segments.Add(new MessageSegment("at", new Dictionary<string, object?>
            {
                { "qq", qq.ToString() ?? string.Empty }
            }));
            // 加空格防止粘连
            _segments.Add(new MessageSegment("text", new Dictionary<string, object?>
            {
                { "text", " " }
            }));
        }
        return this;
    }

    /// <summary>
    /// @全体成员
    /// </summary>
    public MessageChain AtAll()
    {
        _segments.Add(new MessageSegment("at", new Dictionary<string, object?>
        {
            { "qq", "all" }
        }));
        // 加空格防止粘连
        _segments.Add(new MessageSegment("text", new Dictionary<string, object?>
        {
            { "text", " " }
        }));
        return this;
    }

    /// <summary>
    /// 图片消息
    /// </summary>
    /// <param name="file">本地路径/URL/Base64</param>
    /// <param name="summary">图片摘要</param>
    /// <param name="subType">图片子类型，0 普通图片 1 表情包</param>
    public MessageChain Image(string file, string summary = "[图片]", int subType = 0)
    {
        _segments.Add(new MessageSegment("image", new Dictionary<string, object?>
        {
            { "file", file },
            {"summary", summary },
            {"sub_type", subType }
        }));
        return this;
    }

    /// <summary>
    /// 表情消息 (Face)
    /// </summary>
    /// <param name="id">表情ID</param>
    public MessageChain Face(int id)
    {
        _segments.Add(new MessageSegment("face", new Dictionary<string, object?>
        {
            { "id", id }
        }));
        return this;
    }

    /// <summary>
    /// 回复消息 (Reply)
    /// Zod 定义为 number，但建议支持 string 以防万一，这里强制转 int 符合你的 Zod
    /// </summary>
    public MessageChain Reply(int messageId)
    {
        // Reply需要放在最前面
        _segments.Insert(0, new MessageSegment("reply", new Dictionary<string, object?>
        {
            { "id", messageId }
        }));
        return this;
    }

    /// <summary>
    /// 语音消息 (Record)
    /// </summary>
    public MessageChain Record(string file)
    {
        _segments.Add(new MessageSegment("record", new Dictionary<string, object?>
        {
            { "file", file }
        }));
        return this;
    }
    
    /// <summary>
    /// 文件消息 (File)
    /// </summary>
    /// <param name="file">文件路径/url/base64/DataUrl</param>
    /// <returns></returns>
    public MessageChain File(string file)
    {
        _segments.Add(new MessageSegment("file", new Dictionary<string, object?>
        {
            { "file", file }
        }));
        return this;
    }
    
    /// <summary>
    /// 视频消息 (Video)
    /// </summary>
    public MessageChain Video(string file)
    {
        _segments.Add(new MessageSegment("video", new Dictionary<string, object?>
        {
            { "file", file }
        }));
        return this;
    }

    /// <summary>
    /// 音乐分享 (Music) - 平台类
    /// </summary>
    /// <param name="platform">qq 或 163</param>
    public MessageChain Music(string platform, string id)
    {
        _segments.Add(new MessageSegment("music", new Dictionary<string, object?>
        {
            { "type", platform },
            { "id", id }
        }));
        return this;
    }

    /// <summary>
    /// 音乐分享 (Music) - 自定义类
    /// </summary>
    public MessageChain CustomMusic(string url, string audio, string title, string image = "")
    {
        _segments.Add(new MessageSegment("music", new Dictionary<string, object?>
        {
            { "type", "custom" },
            { "url", url },
            { "audio", audio },
            { "title", title },
            { "image", image }
        }));
        return this;
    }

    /// <summary>
    /// JSON 卡片消息
    /// </summary>
    public MessageChain Json(string jsonString)
    {
        _segments.Add(new MessageSegment("json", new Dictionary<string, object?>
        {
            { "data", jsonString }
        }));
        return this;
    }

    /// <summary>
    /// 掷骰子
    /// </summary>
    public MessageChain Dice()
    {
        _segments.Add(new MessageSegment("dice", null)); // Data 为空或空对象
        return this;
    }

    /// <summary>
    /// 猜拳
    /// </summary>
    public MessageChain Rps()
    {
        _segments.Add(new MessageSegment("rps", null));
        return this;
    }

    // === 高级：合并转发节点 (Node) ===
    
    /// <summary>
    /// 添加一个合并转发节点
    /// </summary>
    /// <param name="userId">发送者QQ</param>
    /// <param name="nickname">发送者昵称</param>
    /// <param name="contentChain">嵌套的消息链</param>
    public MessageChain Node(string userId, string nickname, MessageChain contentChain)
    {
        _segments.Add(new MessageSegment("node", new Dictionary<string, object?>
        {
            { "user_id", userId },
            { "nickname", nickname },
            { "content", contentChain.ToList() } // 递归序列化
        }));
        return this;
    }
    
    // 为了方便直接传入 List<MessageSegment>
    public MessageChain Node(string userId, string nickname, List<MessageSegment> content)
    {
        _segments.Add(new MessageSegment("node", new Dictionary<string, object?>
        {
            { "user_id", userId },
            { "nickname", nickname },
            { "content", content }
        }));
        return this;
    }

    /// <summary>
    /// 将构建器转换为 List，这是最终 API 需要的类型
    /// </summary>
    public List<MessageSegment> ToList() => _segments;

    // 隐式转换：允许直接把 MessageChain 赋值给 List<MessageSegment> 参数
    public static implicit operator List<MessageSegment>(MessageChain chain) 
        => chain._segments;
}

/// <summary>
/// 对应 OneBot 协议中的单个消息段
/// </summary>
public class MessageSegment
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, object?> Data { get; set; } = [];

    internal MessageSegment(string type, Dictionary<string, object?>? data = null)
    {
        Type = type;
        Data = data ?? [];
    }
}