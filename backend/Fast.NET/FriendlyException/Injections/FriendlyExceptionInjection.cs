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

using Fast.IaaS;
using Fast.SpecificationProcessor.FriendlyException.Filters;
using Fast.SpecificationProcessor.FriendlyException.Handlers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Fast.Cache.Injections;

/// <summary>
/// <see cref="FriendlyExceptionInjection"/> 友好异常注入
/// </summary>
public class FriendlyExceptionInjection : IApiHostingStartup
{
    /// <summary>
    /// 排序
    /// </summary>
#pragma warning disable CA1822
    public int Order => 69977;
#pragma warning restore CA1822

    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((hostContext, services) =>
        {
            Debugging.Info("Registering friendly exception......");

            // 查找全局异常处理实现类
            var globalExceptionHandler =
                IaaSContext.EffectiveTypes.FirstOrDefault(f =>
                    typeof(IGlobalExceptionHandler).IsAssignableFrom(f) && !f.IsInterface);

            if (globalExceptionHandler != null)
            {
                // 注册全局异常处理实现类
                services.AddSingleton(typeof(IGlobalExceptionHandler), globalExceptionHandler);
            }

            services.Configure<MvcOptions>(options => { options.Filters.Add<FriendlyExceptionFilter>(); });
        });
    }
}