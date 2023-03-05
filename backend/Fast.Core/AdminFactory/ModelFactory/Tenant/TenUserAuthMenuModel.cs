﻿using Fast.SqlSugar.Tenant.AttributeFilter;
using Fast.SqlSugar.Tenant.BaseModel.Interface;
using Fast.SqlSugar.Tenant.Internal.Enum;

namespace Fast.Core.AdminFactory.ModelFactory.Tenant;

/// <summary>
/// 租户用户授权菜单表Model类
/// </summary>
[SugarTable("Ten_User_Auth_Menu", "租户用户授权菜单表")]
[SugarDbType(SugarDbTypeEnum.Tenant)]
public class TenUserAuthMenuModel : IDbEntity
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id", IsNullable = false)]
    public long SysUserId { get; set; }

    /// <summary>
    /// 菜单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单Id", IsNullable = false)]
    public long SysMenuId { get; set; }
}