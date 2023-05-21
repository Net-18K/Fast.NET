﻿using Fast.Admin.Model.Enum;
using Fast.Admin.Model.Model.Sys.Dic;
using SqlSugar;

namespace Fast.Admin.Model.Model.Tenant.Dic;

/// <summary>
/// 租户字典类型表Model类
/// </summary>
[SugarTable("Ten_Dict_Type", "系统字典类型表")]
[SugarDbType(SugarDbTypeEnum.Tenant)]
public class TenDictTypeModel : SysDictTypeModel
{
    /// <summary>
    /// 数据集合
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(TenDictDataModel.TypeId))]
    public new List<TenDictDataModel> DataList { get; set; }
}