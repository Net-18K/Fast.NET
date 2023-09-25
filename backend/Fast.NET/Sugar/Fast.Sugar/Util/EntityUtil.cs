﻿using System.ComponentModel;
using System.Reflection;
using Fast.Core;
using Fast.Sugar.Attributes;
using Fast.Sugar.BaseModel;
using Fast.Sugar.BaseModel.Interface;
using Fast.Sugar.Internal;
using SqlSugar;

namespace Fast.Sugar.Util;

/// <summary>
/// 数据库实体工具类
/// </summary>
static class EntityUtil
{
    /// <summary>
    /// 缓存 Entity 类型
    /// </summary>
    private static List<SugarEntityTypeInfo> _cacheEntityTypeList { get; set; }

    /// <summary>
    /// 清空缓存 Entity 类型
    /// </summary>
    public static void ClearCacheEntityType()
    {
        _cacheEntityTypeList = null;
    }

    /// <summary>
    /// 反射获取所有的数据库Model Type
    /// </summary>
    /// <returns></returns>
    public static List<SugarEntityTypeInfo> ReflexGetAllTEntityList()
    {
        var excludeBaseTypes = new List<Type>
        {
            typeof(IDbEntity),
            typeof(IBaseEntity),
            typeof(IBaseLogEntity),
            typeof(IBaseTenant),
            typeof(IdentityEntity),
            typeof(BaseEntity),
            typeof(BaseTenant),
            typeof(BaseTEntity),
            typeof(PrimaryKeyEntity),
            typeof(ConnectionConfig)
        };

        // 先从缓存获取
        var entityTypeList = _cacheEntityTypeList;
        if (entityTypeList != null && entityTypeList.Any())
            return entityTypeList;

        // 获取所有实现了接口的类的类型
        var types = App.EffectiveTypes.Where(wh => wh.GetInterfaces().Contains(typeof(IDbEntity)))
            // 排除BaseEntity
            .Where(wh => !excludeBaseTypes.Contains(wh));

        // 遍历获取到的类型集合
        entityTypeList = (from type in types
            let dbTypeAttribute = type.GetCustomAttribute<SugarDbTypeAttribute>(true)
            let dbType = dbTypeAttribute?.DbType ?? SugarDbTypeEnum.Default.GetHashCode()
            let dbTypeName =
                dbTypeAttribute?.DbTypeName ?? SugarDbTypeEnum.Default.GetType().GetMember(SugarDbTypeEnum.Default.ToString())
                    .FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description
            let isSplitTable = type.GetCustomAttribute<SplitTableAttribute>(true) != null
            select new SugarEntityTypeInfo(type.Name, dbType, dbTypeName, isSplitTable, type)).ToList();
        // 放入缓存
        _cacheEntityTypeList = entityTypeList;

        return entityTypeList;
    }

    /// <summary>
    /// 反射获取所有的数据库Model
    /// </summary>
    /// <returns></returns>
    public static SugarEntityTypeInfo ReflexGetAllTEntity(string name)
    {
        var entityType = ReflexGetAllTEntityList().FirstOrDefault(f => f.ClassName == name);
        if (entityType == null)
        {
            throw new SqlSugarException("SqlSugar配置错误，请检查 Model 是否继承了接口！");
        }

        return entityType;
    }
}