using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Requests;

/// <summary>
/// 通用请求（无强类型参数）
/// </summary>
public class RawRequest(string action, object? parameters = null, string? echo = null)
    : RequestBase<object?>(action, parameters ?? new object(), echo)
{
}
