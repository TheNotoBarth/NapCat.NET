using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Requests;


/// <summary>
/// 请求数据结构基类
/// </summary>
/// <typeparam name="TParams">请求参数数据结构类型</typeparam>
public abstract class RequestBase<TParams>
{
    protected RequestBase(string action, TParams @params, string? echo = null)
    {
        Action = action;
        Params = @params;
        Echo = string.IsNullOrWhiteSpace(echo) ? Guid.NewGuid().ToString("N") : echo;   // 默认生成一个随机 echo
    }

    [JsonPropertyName("action")]
    public string Action { get; }

    [JsonPropertyName("params")]
    public TParams Params { get; }

    [JsonPropertyName("echo")]
    public string Echo { get; }
}