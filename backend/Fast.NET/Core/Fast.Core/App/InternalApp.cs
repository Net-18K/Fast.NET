﻿using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Fast.Core.DependencyInjection.Extensions;
using Fast.Core.Extensions;
using Fast.Core.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Fast.Core;

/// <summary>
/// 内部 App 副本
/// </summary>
internal class InternalApp
{
    /// <summary>
    /// 应用服务
    /// </summary>
    internal static IServiceCollection InternalServices;

    /// <summary>
    /// 根服务
    /// </summary>
    internal static IServiceProvider RootServices;

    /// <summary>
    /// 配置对象
    /// </summary>
    internal static IConfiguration Configuration;

    /// <summary>
    /// 获取Web主机环境
    /// </summary>
    internal static IWebHostEnvironment WebHostEnvironment;

    /// <summary>
    /// 应用有效程序集
    /// </summary>
    internal static readonly IEnumerable<Assembly> Assemblies;

    /// <summary>
    /// 有效程序集类型
    /// </summary>
    internal static readonly IEnumerable<Type> EffectiveTypes;

    /// <summary>
    /// 未托管的对象集合
    /// </summary>
    internal static readonly ConcurrentBag<IDisposable> UnmanagedObjects;

    /// <summary>
    /// 默认配置文件扫描目录
    /// </summary>
    internal static IEnumerable<string> InternalConfigurationScanDirectories => new[] {"AppConfig", "JsonConfig"};

    /// <summary>
    /// GC 回收默认间隔
    /// </summary>
    internal const int GC_COLLECT_INTERVAL_SECONDS = 5;

    /// <summary>
    /// 记录最近 GC 回收时间
    /// </summary>
    internal static DateTime? LastGCCollectTime { get; set; }

    /// <summary>
    /// <see cref="InternalApp"/>
    /// </summary>
    static InternalApp()
    {
        // 未托管的对象
        UnmanagedObjects = new ConcurrentBag<IDisposable>();

        // 加载程序集
        Assemblies = GetAssemblies();

        // 获取有效的类型集合
        EffectiveTypes = Assemblies.SelectMany(GetTypes);
    }

    /// <summary>
    /// 获取程序集
    /// </summary>
    /// <returns></returns>
    static IEnumerable<Assembly> GetAssemblies()
    {
        // 获取入口程序集
        var entryAssembly = Assembly.GetEntryAssembly();

        // 获取入口程序集所引用的所有程序集
        var referencedAssemblies = entryAssembly?.GetReferencedAssemblies();

        // 加载引用的程序集
        var assemblies = referencedAssemblies.Select(Assembly.Load).ToList();

        // 将入口程序集也放入集合
        assemblies.Add(entryAssembly);

        return assemblies;
    }

    /// <summary>
    /// 加载程序集中的所有类型
    /// </summary>
    /// <param name="ass"></param>
    /// <returns></returns>
    static IEnumerable<Type> GetTypes(Assembly ass)
    {
        var types = Array.Empty<Type>();

        try
        {
            types = ass.GetTypes();
        }
        catch
        {
            Console.WriteLine($"Error load `{ass.FullName}` assembly.");
        }

        return types.Where(u => u.IsPublic);
    }

    /// <summary>
    /// 处理获取对象异常问题
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="action">获取对象委托</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>T</returns>
    internal static T CatchOrDefault<T>(Func<T> action, T defaultValue = null) where T : class
    {
        try
        {
            return action();
        }
        catch
        {
            return defaultValue;
        }
    }

    internal static void ConfigureApplication(IWebHostBuilder builder)
    {
        // 自动装载配置
        builder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
        {
            // 存储环境对象
            WebHostEnvironment = hostContext.HostingEnvironment;

            // 加载配置
            AddJsonFiles(configurationBuilder, hostContext.HostingEnvironment);
        });

        // 应用初始化服务
        builder.ConfigureServices((hostContext, services) =>
        {
            // 存储配置对象
            Configuration = hostContext.Configuration;

            // 存储服务提供器
            InternalServices = services;

            // 注册 Startup 过滤器
            services.AddTransient<IStartupFilter, StartupFilter>();

            // 跨域配置
            services.AddCorsAccessor(hostContext.Configuration);

            // 注册 HttpContextAccessor 服务
            services.AddHttpContextAccessor();

            // JSON 序列化配置
            services.AddJsonOptions();

            // 注册全局依赖注入
            services.AddInnerDependencyInjection();

            // 添加对象映射
            services.AddObjectMapper();

            // 默认内置 GBK，Windows-1252, Shift-JIS, GB2312 编码支持
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        });
    }

    private static void AddJsonFiles(IConfigurationBuilder configurationBuilder, IHostEnvironment hostEnvironment)
    {
        // 获取程序执行目录
        var executeDirectory = AppContext.BaseDirectory;

        // 扫描自定义配置扫描目录
        var jsonFiles = new[] {executeDirectory}.Concat(InternalConfigurationScanDirectories).Where(Directory.Exists)
            .SelectMany(s => Directory.GetFiles(s, "*.json", SearchOption.TopDirectoryOnly)).ToList();

        // 如果没有配置文件，中止执行
        if (!jsonFiles.Any())
            return;

        // 获取环境变量名，如果没找到，则读取 NETCORE_ENVIRONMENT 环境变量信息识别（用于非 Web 环境）
        var envName = hostEnvironment?.EnvironmentName ?? Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Unknown";

        // 处理控制台应用程序
        var _excludeJsonPrefixArr = hostEnvironment == default
            ? excludeJsonPrefixArr.Where(u => !u.Equals("appsettings"))
            : excludeJsonPrefixArr;

        // 将所有文件进行分组
        var jsonFilesGroups = SplitConfigFileNameToGroups(jsonFiles).Where(u =>
            !_excludeJsonPrefixArr.Contains(u.Key, StringComparer.OrdinalIgnoreCase) && !u.Any(c =>
                runtimeJsonSuffixArr.Any(z => c.EndsWith(z, StringComparison.OrdinalIgnoreCase))));

        // 遍历所有配置分组
        foreach (var group in jsonFilesGroups)
        {
            // 限制查找的 json 文件组
            var limitFileNames = new[] {$"{group.Key}.json", $"{group.Key}.{envName}.json"};

            // 查找默认配置和环境配置
            var files = group.Where(u => limitFileNames.Contains(Path.GetFileName(u), StringComparer.OrdinalIgnoreCase))
                .OrderBy(u => Path.GetFileName(u).Length);

            // 循环加载
            foreach (var jsonFile in files)
            {
                configurationBuilder.AddJsonFile(jsonFile, optional: true, reloadOnChange: true);
            }
        }
    }

    /// <summary>
    /// 排除的配置文件前缀
    /// </summary>
    private static readonly string[] excludeJsonPrefixArr = {"appsettings", "bundleconfig", "compilerconfig"};

    /// <summary>
    /// 排除运行时 Json 后缀
    /// </summary>
    private static readonly string[] runtimeJsonSuffixArr =
    {
        "deps.json", "runtimeconfig.dev.json", "runtimeconfig.prod.json", "runtimeconfig.json", "staticwebassets.runtime.json"
    };

    /// <summary>
    /// 对配置文件名进行分组
    /// </summary>
    /// <param name="configFiles"></param>
    /// <returns></returns>
    private static IEnumerable<IGrouping<string, string>> SplitConfigFileNameToGroups(IEnumerable<string> configFiles)
    {
        // 分组
        return configFiles.GroupBy(Function);

        // 本地函数
        static string Function(string file)
        {
            // 根据 . 分隔
            var fileNameParts = Path.GetFileName(file).Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (fileNameParts.Length == 2)
                return fileNameParts[0];

            return string.Join('.', fileNameParts.Take(fileNameParts.Length - 2));
        }
    }
}