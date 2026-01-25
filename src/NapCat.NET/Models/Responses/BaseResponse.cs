using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Responses;


/// <summary>
/// 响应数据结构基类
/// </summary>
/// <typeparam name="T">Data 类型</typeparam>
public class BaseResponse<T>
{
    /// <summary>
    /// 请求状态，ok或error
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// 响应码
    /// </summary>
    [JsonPropertyName("retcode")]
    public int Retcode { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 给人看的提示信息
    /// </summary>
    [JsonPropertyName("wording")]
    public string? Wording { get; set; }

    /// <summary>
    /// 请求标识
    /// </summary>
    [JsonPropertyName("echo")]
    public string? Echo { get; set; }
}