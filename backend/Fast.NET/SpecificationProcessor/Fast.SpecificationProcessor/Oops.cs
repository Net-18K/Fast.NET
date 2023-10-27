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

using Fast.NET;

// ReSharper disable once CheckNamespace
namespace Fast.SpecificationProcessor;

/// <summary>
/// 抛异常静态类
/// </summary>
public static class Oops
{
    /// <summary>
    /// 抛出业务异常信息
    /// </summary>
    /// <param name="errorMessage"><see cref="string"/>异常消息</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns><see cref="UserFriendlyException"/>异常实例</returns>
    [Obsolete("This method is deprecated, use throw new UserFriendlyException() instead.")]
    public static UserFriendlyException Bah(string errorMessage, params object[] args)
    {
        return new UserFriendlyException(string.Format(errorMessage, args));
    }

    /// <summary>
    /// 抛出字符串异常
    /// </summary>
    /// <param name="errorMessage"><see cref="string"/>异常消息</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns><see cref="Exception"/>异常实例</returns>
    [Obsolete("This method is deprecated, use throw new Exception() instead.")]
    public static Exception Oh(string errorMessage, params object[] args)
    {
        return new Exception(string.Format(errorMessage, args));
    }

    /// <summary>
    /// 抛出字符串异常
    /// </summary>
    /// <param name="errorMessage"><see cref="string"/>异常消息</param>
    /// <param name="exceptionType">具体异常类型</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns><see cref="Exception"/>异常实例</returns>
    [Obsolete("This method is deprecated, use throw new Exception() instead.")]
    public static Exception Oh(string errorMessage, Type exceptionType, params object[] args)
    {
        var exceptionMessage = string.Format(errorMessage, args);
        return new Exception(exceptionMessage,
            innerException: Activator.CreateInstance(exceptionType, exceptionMessage) as Exception);
    }

    /// <summary>
    /// 抛出字符串异常
    /// </summary>
    /// <typeparam name="TException">具体异常类型</typeparam>
    /// <param name="errorMessage"><see cref="string"/>异常消息</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns><see cref="Exception"/>异常实例</returns>
    [Obsolete("This method is deprecated, use throw new Exception() instead.")]
    public static Exception Oh<TException>(string errorMessage, params object[] args) where TException : class
    {
        return Oh(errorMessage, typeof(TException), args);
    }
}