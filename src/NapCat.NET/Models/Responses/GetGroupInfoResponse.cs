using System.Text.Json.Serialization;

namespace NapCat.NET.Models.Responses;

/// <summary>
/// 群信息响应数据结构
/// </summary>
public class GetGroupInfoResponse
{
    /// <summary>
    /// 群全员禁言状态，1 全员禁言，0 未全员禁言
    /// </summary>
    [JsonPropertyName("group_all_shut")]
    public int GroupAllShut { get; set; }
    
    /// <summary>
    /// 群备注
    /// </summary>
    [JsonPropertyName("group_remark")]
    public string? GroupRemark { get; set; }

    /// <summary>
    /// 群号
    /// </summary>
    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    /// <summary>
    /// 群名称
    /// </summary>
    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }

    /// <summary>
    /// 群成员数量
    /// </summary>
    [JsonPropertyName("member_count")]
    public int MemberCount { get; set; }

    /// <summary>
    /// 群最大成员数量
    /// </summary>
    [JsonPropertyName("max_member_count")]
    public int MaxMemberCount { get; set; }
}