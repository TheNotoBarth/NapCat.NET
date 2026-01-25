using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Requests;

/// <summary>
/// 获取群信息请求数据结构
/// </summary>
/// <param name="groupId">群号</param>
/// <param name="echo"></param>
public class GetGroupInfoRequest(long groupId, string? echo = null) 
    : RequestBase<GetGroupInfoParams>(ApiName, new GetGroupInfoParams(groupId), echo)
{
    public const string ApiName = "get_group_info";
}

public class GetGroupInfoParams(long groupId)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;
}