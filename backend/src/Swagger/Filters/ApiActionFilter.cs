﻿// Apache开源许可证
//
// 版权所有 © 2018-Now 小方
//
// 特此免费授予获得本软件及其相关文档文件（以下简称“软件”）副本的任何人以处理本软件的权利，
// 包括但不限于使用、复制、修改、合并、发布、分发、再许可、销售软件的副本，
// 以及允许拥有软件副本的个人进行上述行为，但须遵守以下条件：
//
// 在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
//
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，
// 无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

using System.Reflection;
using Fast.DynamicApplication;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fast.Swagger.Filters;

/// <summary>
/// 规范化文档自定义更多功能
/// </summary>
internal class ApiActionFilter : IOperationFilter
{
    /// <summary>
    /// 实现过滤器方法
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 获取方法
        var method = context.MethodInfo;

        // 处理更多描述
        if (method.IsDefined(typeof(ApiDescriptionSettingsAttribute), true))
        {
            var apiDescriptionSettings = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);

            // 添加单一接口描述
            if (!string.IsNullOrWhiteSpace(apiDescriptionSettings.Description))
            {
                operation.Description += apiDescriptionSettings.Description;
            }
        }

        // 处理过时
        if (method.IsDefined(typeof(ObsoleteAttribute), true))
        {
            var deprecated = method.GetCustomAttribute<ObsoleteAttribute>(true);
            if (!string.IsNullOrWhiteSpace(deprecated.Message))
            {
                operation.Description = $"<div>{deprecated.Message}</div>" + operation.Description;
            }
        }
    }
}