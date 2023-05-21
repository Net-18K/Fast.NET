﻿namespace Fast.Admin.Model.BaseModel.Dto;

/// <summary>
/// 通用输出参数
/// </summary>
public class BaseOutput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedTime { get; set; }
}