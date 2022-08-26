﻿namespace Fast.Core.AdminFactory.BaseModelFactory;

/// <summary>
/// 记录基类实现
/// </summary>
public class BaseLogEntity : IBaseLogEntity, IPrimaryKeyEntity<int>, IDbEntity
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [SugarColumn(IsIdentity = true, ColumnDescription = "Id主键", IsPrimaryKey = true)] //通过特性设置主键和自增列 
    // 注意是在这里定义你的公共实体
    public virtual int Id { get; set; }

    /// <summary>
    /// 手机型号
    /// </summary>
    [SugarColumn(ColumnDescription = "手机型号", ColumnDataType = "Nvarchar(100)", IsNullable = true)]
    public virtual string PhoneModel { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    [SugarColumn(ColumnDescription = "操作系统", ColumnDataType = "Nvarchar(100)", IsNullable = true)]
    public virtual string OS { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    [SugarColumn(ColumnDescription = "浏览器", ColumnDataType = "Nvarchar(100)", IsNullable = true)]
    public virtual string Browser { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    [SugarColumn(ColumnDescription = "省份", ColumnDataType = "Nvarchar(20)", IsNullable = true)]
    public virtual string Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [SugarColumn(ColumnDescription = "城市", ColumnDataType = "Nvarchar(20)", IsNullable = true)]
    public virtual string City { get; set; }

    /// <summary>
    /// 运营商
    /// </summary>
    [SugarColumn(ColumnDescription = "运营商", ColumnDataType = "Nvarchar(20)", IsNullable = true)]
    public virtual string Operator { get; set; }

    /// <summary>
    /// Ip
    /// </summary>
    [SugarColumn(ColumnDescription = "Ip", ColumnDataType = "Nvarchar(15)", IsNullable = true)]
    public virtual string Ip { get; set; }

    /// <summary>
    /// 记录表创建
    /// </summary>
    public virtual void RecordCreate()
    {
        var userAgentInfo = HttpNewUtil.UserAgentInfo();
        var wanInfo = HttpNewUtil.WanInfo(HttpNewUtil.Ip).Result;

        PhoneModel = userAgentInfo.PhoneModel;
        OS = userAgentInfo.OS;
        Browser = userAgentInfo.Browser;
        Province = wanInfo.Pro;
        City = wanInfo.City;
        Operator = wanInfo.Operator;
        Ip = wanInfo.Ip;
    }
}