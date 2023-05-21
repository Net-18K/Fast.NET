﻿using Fast.SqlSugar.BaseModel.Interface;

namespace Fast.SqlSugar.BaseModel;

/// <summary>
/// 租户基类
/// </summary>
public class BaseTenant : IBaseTenant
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsNullable = true, CreateTableFieldSort = 997)]
    public virtual long TenantId { get; set; }
}