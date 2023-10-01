﻿using Fast.UnifyResult.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace Fast.UnifyResult.Filters;

/// <summary>
/// 规范化结构（请求成功）过滤器
/// </summary>
public class SucceededUnifyResultFilter : IAsyncActionFilter, IOrderedFilter
{
    /// <summary>
    /// 过滤器排序
    /// </summary>
    private const int FilterOrder = 8888;

    /// <summary>
    /// 排序属性
    /// </summary>
    public int Order => FilterOrder;

    /// <summary>
    /// 处理规范化结果
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 执行 Action 并获取结果
        var actionExecutedContext = await next();

        // 排除 WebSocket 请求处理
        if (context.HttpContext.WebSockets.IsWebSocketRequest || context.HttpContext.Request.Path == "/ws")
            return;

        // 获取规范化处理器
        var unifyResult = context.HttpContext.RequestServices.GetService<IUnifyResultProvider>();

        // 如果为空，则不走下面的逻辑
        if (unifyResult == null)
            return;

        // 处理已经含有状态码结果的 Result
        if (actionExecutedContext.Result is IStatusCodeActionResult statusCodeResult && statusCodeResult.StatusCode != null)
        {
            // 小于 200 或者 大于 299 都不是成功值，直接跳过
            if (statusCodeResult.StatusCode.Value < 200 || statusCodeResult.StatusCode.Value > 299)
            {
                // 处理规范化结果
                var httpContext = context.HttpContext;
                var statusCode = statusCodeResult.StatusCode.Value;

                // 解决刷新 Token 时间和 Token 时间相近问题
                if (statusCodeResult.StatusCode.Value == StatusCodes.Status401Unauthorized &&
                    httpContext.Response.Headers.ContainsKey("access-token") &&
                    httpContext.Response.Headers.ContainsKey("x-access-token"))
                {
                    httpContext.Response.StatusCode = statusCode = StatusCodes.Status403Forbidden;
                }

                // 如果 Response 已经完成输出，则禁止写入
                if (httpContext.Response.HasStarted)
                    return;
                await unifyResult.OnResponseStatusCodes(httpContext, statusCode);

                return;
            }
        }

        // 如果出现异常，则不会进入该过滤器
        if (actionExecutedContext.Exception != null)
            return;

        // 处理 BadRequestObjectResult 类型规范化处理
        if (actionExecutedContext.Result is BadRequestObjectResult badRequestObjectResult)
        {
            var result = unifyResult.OnValidateFailed(context, badRequestObjectResult.Value);
            if (result != null)
                actionExecutedContext.Result = result;
        }
        else
        {
            IActionResult result = default;

            // 排除以下结果，跳过规范化处理
            var isDataResult = actionExecutedContext.Result switch
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
            {
                var data = actionExecutedContext.Result switch
                {
                    // 处理内容结果
                    ContentResult content => content.Content,
                    // 处理对象结果
                    ObjectResult obj => obj.Value,
                    // 处理 JSON 对象
                    JsonResult json => json.Value,
                    _ => null,
                };
                result = unifyResult.OnSucceeded(actionExecutedContext, data);
            }

            // 如果是不能规范化的结果类型，则跳过
            if (result == null)
                return;

            actionExecutedContext.Result = result;
        }
    }
}