﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Fast.Cache;
using Fast.Core.SqlSugar.Internal;
using Fast.Iaas;
using Fast.Iaas.Attributes;
using Fast.Sugar.Util;
using Fast.User;
using Furion;
using SqlSugar;
using ConnectionConfigOption = Fast.Sugar.Internal.ConnectionConfigOption;

namespace Fast.Core.SqlSugar.Extension;

/// <summary>
/// SqlSugar扩展类
/// </summary>
public static class Extension
{
    private static ConnectionConfigOption DefaultDataBaseInfo { get; set; }

    /// <summary>
    /// 获取默认连接配置
    /// </summary>
    /// <returns></returns>
    public static ConnectionConfigOption GetDefaultDataBaseInfo()
    {
        if (DefaultDataBaseInfo != null)
        {
            return DefaultDataBaseInfo;
        }

        DefaultDataBaseInfo = FastContext.GetConfig<ConnectionConfigOption>("ConnectionStrings");

        return DefaultDataBaseInfo;
    }

    /// <summary>
    /// LoadSqlSugar，支持传入租户Id，获取租户Id的DbClient
    /// 默认是当前登录用户的租户Id
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="db"></param>
    /// <param name="_user"></param>
    /// <param name="_cache"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public static (ISqlSugarClient db, ConnectionConfigOption dataBaseInfo) LoadSqlSugar<TEntity>(this ISqlSugarClient db,
        IUser _user,ICache _cache,
        long? tenantId = null) where TEntity : class, new()
    {
        var _db = (SqlSugarClient) db;

        var dbType = EntityUtil.ReflexGetAllTEntity(typeof(TEntity).Name);

        var defaultDataBaseInfo = GetDefaultDataBaseInfo();

        // 默认Db
        var defaultDb = _db.GetConnection(defaultDataBaseInfo.ConnectionId);

        // 判断是否为租户库
        if (dbType.DbType == SugarDbTypeEnum.Default.GetHashCode())
        {
            return (defaultDb, defaultDataBaseInfo);
        }

        if (tenantId is null or 0)
        {
            // 获取租户Id
            tenantId = _user.TenantId;
        }

        var dbInfo = SqlSugarClientUtil.GetDbInfo(defaultDb, defaultDataBaseInfo.DbInfoTableName, dbType.DbType, tenantId.Value,
            _cache).Result;

        return (SqlSugarClientUtil.GetSqlSugarClient(_db, dbInfo), dbInfo);
    }

    /// <summary>
    /// 获取SugarTable特性中的TableName
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetSugarTableName(this Type type)
    {
        var sugarTable = type.GetCustomAttribute<SugarTable>();
        if (sugarTable != null && !string.IsNullOrEmpty(sugarTable.TableName))
        {
            return sugarTable.TableName;
        }

        return type.Name;
    }

    /// <summary>
    /// 转为DataTable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<DataTable> ToDataTable<T>(this List<T> list)
    {
        var result = new List<DataTable>();

        // 判断是否为空
        if (list == null || !list.Any())
            return result;

        var type = typeof(T);
        if (type.Name == "Object")
        {
            type = list[0].GetType();
        }

        // 获取所有属性
        var properties = type.GetProperties();
        foreach (var item in list)
        {
            var dataTable = new DataTable();

            // 表名赋值
            dataTable.TableName = type.GetSugarTableName();

            var tempList = new ArrayList();

            foreach (var property in properties)
            {
                var colType = property.PropertyType;
                // 泛型
                if (colType.IsGenericType && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    colType = colType.GetGenericArguments()[0];
                }

                // 获取Sugar列特性
                var sugarColumn = property.GetCustomAttribute<SugarColumn>(false);

                // 判断忽略列
                if (sugarColumn?.IsIgnore == true)
                {
                    continue;
                }

                var columnName = sugarColumn?.ColumnName ?? property.Name;

                dataTable.Columns.Add(columnName, colType);

                tempList.Add(property.GetValue(item, null));
            }

            dataTable.LoadDataRow(tempList.ToArray(), true);

            result.Add(dataTable);
        }

        return result;
    }

    /// <summary>
    /// SqlSugar分页扩展
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(this ISugarQueryable<TEntity> queryable, int pageIndex,
        int pageSize)
    {
        var totalCount = 0;
        var items = queryable.ToPageList(pageIndex, pageSize, ref totalCount);
        var totalPages = (int) Math.Ceiling(totalCount / (double) pageSize);
        return new SqlSugarPagedList<TEntity>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPages = pageIndex < totalPages,
            HasPrevPages = pageIndex - 1 > 0
        };
    }

    /// <summary>
    /// SqlSugar分页扩展
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static async Task<SqlSugarPagedList<TEntity>> ToPagedListAsync<TEntity>(this ISugarQueryable<TEntity> queryable,
        int pageIndex, int pageSize)
    {
        RefAsync<int> totalCount = 0;
        var items = await queryable.ToPageListAsync(pageIndex, pageSize, totalCount);
        var totalPages = (int) Math.Ceiling(totalCount / (double) pageSize);
        return new SqlSugarPagedList<TEntity>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Items = items,
            TotalCount = (int) totalCount,
            TotalPages = totalPages,
            HasNextPages = pageIndex < totalPages,
            HasPrevPages = pageIndex - 1 > 0
        };
    }

    /// <summary>
    /// SqlSugar分页扩展
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static SqlSugarPagedList<TResult> ToPagedList<TEntity, TResult>(this ISugarQueryable<TEntity> queryable, int pageIndex,
        int pageSize, Expression<Func<TEntity, TResult>> expression)
    {
        var totalCount = 0;
        var items = queryable.ToPageList(pageIndex, pageSize, ref totalCount, expression);
        var totalPages = (int) Math.Ceiling(totalCount / (double) pageSize);
        return new SqlSugarPagedList<TResult>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPages = pageIndex < totalPages,
            HasPrevPages = pageIndex - 1 > 0
        };
    }

    /// <summary>
    /// SqlSugar分页扩展
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static async Task<SqlSugarPagedList<TResult>> ToPagedListAsync<TEntity, TResult>(
        this ISugarQueryable<TEntity> queryable, int pageIndex, int pageSize, Expression<Func<TEntity, TResult>> expression)
    {
        RefAsync<int> totalCount = 0;
        var items = await queryable.ToPageListAsync(pageIndex, pageSize, totalCount, expression);
        var totalPages = (int) Math.Ceiling(totalCount / (double) pageSize);
        return new SqlSugarPagedList<TResult>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Items = items,
            TotalCount = (int) totalCount,
            TotalPages = totalPages,
            HasNextPages = pageIndex < totalPages,
            HasPrevPages = pageIndex - 1 > 0
        };
    }
}