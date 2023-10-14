﻿// Apache开源许可证
//
// 版权所有 © 2018-2023 1.8K仔
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

using System.Collections.Concurrent;
using System.Reflection;
using Fast.NET;
using Fast.UnifyResult.Attributes;
using Fast.UnifyResult.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fast.UnifyResult.Contexts;

/// <summary>
/// <see cref="UnifyContext"/> 规范化结果上下文
/// </summary>
[SuppressSniffer]
public static class UnifyContext
{
    ///// <summary>
    ///// 是否启用规范化结果
    ///// </summary>
    //internal static bool EnabledUnifyHandler = false;

    ///// <summary>
    ///// 规范化结果额外数据键
    ///// </summary>
    //internal static string UnifyResultExtrasKey = "UNIFY_RESULT_EXTRAS";

    /// <summary>
    /// 规范化结果提供器
    /// </summary>
    internal static ConcurrentDictionary<string, UnifyMetadata> UnifyProviders = new();

    /// <summary>
    /// 设置响应状态码
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/></param>
    /// <param name="statusCode"><see cref="int"/></param>
    /// <param name="return200StatusCodes"><see cref="Array"/> 设置返回 200 状态码列表。只支持 400+(404除外) 状态码</param>
    /// <param name="adaptStatusCodes"><see cref="Array"/> 适配（篡改）状态码。只支持 400+(404除外) 状态码</param>
    /// <remarks>
    /// 示例：
    ///     return200StatusCodes = [401, 403]
    ///     adaptStatusCodes = [[401, 200], [403, 200]]
    /// </remarks>
    public static void SetResponseStatusCodes(HttpContext context, int statusCode, int[] return200StatusCodes = null,
        int[][] adaptStatusCodes = null)
    {
        // 篡改响应状态码
        if (adaptStatusCodes is {Length: > 0})
        {
            var adaptStatusCode = adaptStatusCodes.FirstOrDefault(f => f[0] == statusCode);
            if (adaptStatusCode is {Length: > 0} && adaptStatusCode[0] > 0)
            {
                context.Response.StatusCode = adaptStatusCode[1];
                return;
            }
        }

        // 200 状态码返回
        if (return200StatusCodes is {Length: > 0})
        {
            // 判断当前状态码是否存在与200状态码列表中
            if (return200StatusCodes.Contains(statusCode))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
            }
        }
    }

    /// <summary>
    /// 检查请求成功是否进行规范化处理
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext"/></param>
    /// <param name="method"><see cref="MethodInfo"/></param>
    /// <param name="unifyResult"><see cref="IUnifyResultProvider"/></param>
    /// <param name="isWebRequest"><see cref="bool"/></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    /// <returns><see cref="bool"/></returns>
    internal static bool CheckSucceededNonUnify(HttpContext httpContext, MethodInfo method, out IUnifyResultProvider unifyResult,
        bool isWebRequest = true)
    {
        // 解析规范化元数据
        var unityMetadata = GetMethodUnityMetadata(method);

        // 判断是否跳过规范化处理
        // ReSharper disable once ReplaceWithSingleAssignment.False
        var isSkip = false;

        // 判断返回类型是否包含了规范化处理的返回类型
        if (method.GetRealReturnType().HasImplementedRawGeneric(unityMetadata.ResultType))
        {
            isSkip = true;
        }

        // 这是不使用 method.GetCustomAttribute<NonUnifyAttribute>() != null 的原因是，避免直接继承了 NonUnifyAttribute 使用自定义的特性
        var nonUnifyAttributeType = typeof(NonUnifyAttribute);
        var producesResponseTypeAttributeType = typeof(ProducesResponseTypeAttribute);
        var iApiResponseMetadataProviderType = typeof(IApiResponseMetadataProvider);
        if (!isSkip && method.CustomAttributes.Any(a =>
                // 判断方法头部是否贴有 NonUnifyAttribute 特性
                nonUnifyAttributeType.IsAssignableFrom(a.AttributeType) ||
                // 判断方法头部是否贴有 原生的 HTTP 响应类型的特性 ProducesResponseTypeAttribute
                producesResponseTypeAttributeType.IsAssignableFrom(a.AttributeType) ||
                // 判断方法头部是否贴有 IApiResponseMetadataProvider 特性
                iApiResponseMetadataProviderType.IsAssignableFrom(a.AttributeType)))
        {
            isSkip = true;
        }

        // 判断方法所在的类是否贴有 NonUnifyAttribute 特性
        if (!isSkip && method.ReflectedType?.IsDefined(nonUnifyAttributeType, true) == true)
        {
            isSkip = true;
        }

        // 判断方法所属类型的程序集的名称以 "Microsoft.AspNetCore.OData" 
        if (!isSkip && method.ReflectedType?.Assembly?.GetName()?.Name?.StartsWith("Microsoft.AspNetCore.OData") == true)
        {
            isSkip = true;
        }

        // 判断是否为 Web 请求
        if (!isWebRequest)
        {
            unifyResult = null;
            return isSkip;
        }

        if (isSkip)
        {
            unifyResult = null;
        }
        else
        {
            unifyResult = httpContext.RequestServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
        }

        return unifyResult == null || isSkip;
    }

    /// <summary>
    /// 检查请求失败（验证失败、抛异常）是否进行规范化处理
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext"/></param>
    /// <param name="method"><see cref="MethodInfo"/></param>
    /// <param name="unifyResult"><see cref="IUnifyResultProvider"/></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    /// <returns><see cref="bool"/></returns>
    internal static bool CheckFailedNonUnify(HttpContext httpContext, MethodInfo method, out IUnifyResultProvider unifyResult)
    {
        // 解析规范化元数据
        var unityMetadata = GetMethodUnityMetadata(method);

        // 判断是否跳过规范化处理
        // ReSharper disable once ReplaceWithSingleAssignment.False
        var isSkip = false;

        // 这是不使用 method.GetCustomAttribute<NonUnifyAttribute>() != null 的原因是，避免直接继承了 NonUnifyAttribute 使用自定义的特性
        var nonUnifyAttributeType = typeof(NonUnifyAttribute);

        // 判断方法头部是否贴有 NonUnifyAttribute 特性
        if (method.CustomAttributes.Any(a => nonUnifyAttributeType.IsAssignableFrom(a.AttributeType)))
        {
            isSkip = true;
        }

        var producesResponseTypeAttributeType = typeof(ProducesResponseTypeAttribute);
        var iApiResponseMetadataProviderType = typeof(IApiResponseMetadataProvider);

        if (!isSkip && !method.CustomAttributes.Any(a =>
                // 判断方法头部是否贴有 原生的 HTTP 响应类型的特性 ProducesResponseTypeAttribute
                producesResponseTypeAttributeType.IsAssignableFrom(a.AttributeType) ||
                // 判断方法头部是否贴有 IApiResponseMetadataProvider 特性
                iApiResponseMetadataProviderType.IsAssignableFrom(a.AttributeType)) &&
            // 判断方法所在的类是否贴有 NonUnifyAttribute 特性
            method.ReflectedType?.IsDefined(nonUnifyAttributeType, true) == true)
        {
            isSkip = true;
        }

        // 判断方法所属类型的程序集的名称以 "Microsoft.AspNetCore.OData" 
        if (!isSkip && method.ReflectedType?.Assembly?.GetName()?.Name?.StartsWith("Microsoft.AspNetCore.OData") == true)
        {
            isSkip = true;
        }

        if (isSkip)
        {
            unifyResult = null;
        }
        else
        {
            unifyResult = httpContext.RequestServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
        }

        return unifyResult == null || isSkip;
    }

    /// <summary>
    /// 检查短路状态码（>=400）是否进行规范化处理
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="unifyResult"></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    internal static bool CheckStatusCodeNonUnify(HttpContext httpContext, out IUnifyResultProvider unifyResult)
    {
        // 获取终点路由特性
        var endpointFeature = httpContext.Features.Get<IEndpointFeature>();
        if (endpointFeature == null)
        {
            unifyResult = null;
            return true;
        }

        // 判断是否跳过规范化处理
        // ReSharper disable once ReplaceWithSingleAssignment.False
        var isSkip = false;

        // 判断终点路由是否存在 NonUnifyAttribute 特性
        if (httpContext.GetMetadata<NonUnifyAttribute>() != null)
        {
            isSkip = true;
        }

        // 判断终点路由是否存在 NonUnifyAttribute 特性
        if (!isSkip && endpointFeature?.Endpoint?.Metadata?.GetMetadata<NonUnifyAttribute>() != null)
        {
            isSkip = true;
        }

        // 判断请求头部是否包含 odata.metadata=
        if (!isSkip && httpContext?.Request?.Headers["accept"].ToString()
                ?.Contains("odata.metadata=", StringComparison.OrdinalIgnoreCase) == true)
        {
            isSkip = true;
        }

        // 判断请求头部是否包含 odata.streaming=
        if (!isSkip && httpContext?.Request?.Headers["accept"].ToString()
                ?.Contains("odata.streaming=", StringComparison.OrdinalIgnoreCase) == true)
        {
            isSkip = true;
        }

        if (isSkip)
        {
            unifyResult = null;
        }
        else
        {
            // 解析规范化元数据
            var unifyProviderAttribute = endpointFeature?.Endpoint?.Metadata?.GetMetadata<UnifyProviderAttribute>();
            if (UnifyProviders.TryGetValue(unifyProviderAttribute?.Name ?? string.Empty, out var unityMetadata))
            {
                unifyResult = httpContext.RequestServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
            }
            else
            {
                unifyResult = null;
            }
        }

        return unifyResult == null || isSkip;
    }

    /// <summary>
    /// 检查是否是有效的结果（可进行规范化的结果）
    /// </summary>
    /// <param name="result"><see cref="IActionResult"/></param>
    /// <param name="data"><see cref="object"/></param>
    /// <returns></returns>
    internal static bool CheckValidResult(IActionResult result, out object data)
    {
        data = default;

        // 排除以下结果，跳过规范化处理
        var isDataResult = result switch
        {
            ViewResult => false,
            PartialViewResult => false,
            FileResult => false,
            ChallengeResult => false,
            SignInResult => false,
            SignOutResult => false,
            RedirectToPageResult => false,
            RedirectToRouteResult => false,
            RedirectResult => false,
            RedirectToActionResult => false,
            LocalRedirectResult => false,
            ForbidResult => false,
            ViewComponentResult => false,
            PageResult => false,
            NotFoundResult => false,
            NotFoundObjectResult => false,
            _ => true,
        };

        // 目前支持返回值 ActionResult
        if (isDataResult)
            data = result switch
            {
                // 处理内容结果
                ContentResult content => content.Content,
                // 处理对象结果
                ObjectResult obj => obj.Value,
                // 处理 JSON 对象
                JsonResult json => json.Value,
                _ => null,
            };

        return isDataResult;
    }

    /// <summary>
    /// 获取方法规范化元数据
    /// </summary>
    /// <remarks>如果追求性能，这里理应缓存起来，避免每次请求去检测</remarks>
    /// <param name="method"></param>
    /// <returns></returns>
    internal static UnifyMetadata GetMethodUnityMetadata(MethodInfo method)
    {
        if (method == default)
            return default;

        var unityProviderAttribute = method.GetFoundAttribute<UnifyProviderAttribute>(true);

        // 获取元数据
        var isExists = UnifyProviders.TryGetValue(unityProviderAttribute?.Name ?? string.Empty, out var metadata);
        if (!isExists)
        {
            // 不存在则将默认的返回
            UnifyProviders.TryGetValue(string.Empty, out metadata);
        }

        return metadata;
    }
}