﻿using System.ComponentModel;
using System.Reflection;

namespace Fast.Admin.Model;

/// <summary>
/// Sugar数据库类型特征，区分是那个数据库，默认是Default库（0）
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SugarDbTypeAttribute : Attribute
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public int DbType { get; set; }

    /// <summary>
    /// 数据库类型名称
    /// </summary>
    public string DbTypeName { get; set; }

    /// <summary>
    /// Sugar数据库类型特征，区分是那个数据库，默认是Default库（0）
    /// </summary>
    public SugarDbTypeAttribute()
    {
        DbType = SugarDbTypeEnum.Default.GetHashCode();

        DbTypeName = SugarDbTypeEnum.Default.GetType().GetMember(SugarDbTypeEnum.Default.ToString() ?? string.Empty)
            .FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }

    /// <summary>
    /// Sugar数据库类型特征，区分是那个数据库，默认是Default库（0）
    /// </summary>
    /// <param name="dbType">数据库类型（Int类型）</param>
    /// <param name="dbTypeName">数据库类型名称</param>
    public SugarDbTypeAttribute(int dbType, string dbTypeName = null)
    {
        DbType = dbType;
        DbTypeName = dbTypeName;
    }

    /// <summary>
    /// Sugar数据库类型特征，区分是那个数据库，默认是Default库（0）
    /// </summary>
    /// <param name="dbType">数据库类型（SugarDbTypeEnum类型）</param>
    public SugarDbTypeAttribute(SugarDbTypeEnum dbType)
    {
        DbType = dbType.GetHashCode();
        DbTypeName = dbType.GetType().GetMember(SugarDbTypeEnum.Default.ToString() ?? string.Empty).FirstOrDefault()
            ?.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }
}