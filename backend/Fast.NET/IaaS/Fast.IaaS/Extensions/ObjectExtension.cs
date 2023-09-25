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

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fast.IaaS.Extensions;

/// <summary>
/// <see cref="object"/> 拓展类
/// </summary>
public static class ObjectExtension
{
    /// <summary>
    /// 将一个对象转化为 Get 请求的String字符串
    /// 注：List，Array，Object属性不支持
    /// </summary>
    /// <param name="obj"><see cref="object"/></param>
    /// <param name="isToLower">首字母是否小写</param>
    /// <returns><see cref="string"/></returns>
    public static string ToQueryString(this object obj, bool isToLower = false)
    {
        if (obj == null)
            return string.Empty;

        var dictionary = new Dictionary<string, string>();

        var t = obj.GetType(); // 获取对象对应的类， 对应的类型

        var pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性

        foreach (var p in pi)
        {
            var m = p.GetGetMethod();

            if (m == null || !m.IsPublic)
                continue;
            // 进行判NULL处理
            if (m.Invoke(obj, new object[] { }) == null)
                continue;

            var value = m.Invoke(obj, new object[] { });

            // 进行List集合处理
            var valType = value?.GetType();
            if (valType is {IsGenericType: true})
            {
                // 这里如果还有别的参数，需要再次添加
                switch (value)
                {
                    case List<string> strList:
                        var strListVal = strList.Aggregate("",
                            (current, item) => current + $"{item}&{(isToLower ? p.Name.FirstCharToLower() : p.Name)}[]=");

                        strListVal = strListVal[..^$"&{(isToLower ? p.Name.FirstCharToLower() : p.Name)}[]=".Length];

                        dictionary.Add($"{(isToLower ? p.Name.FirstCharToLower() : p.Name)}[]", strListVal); // 向字典添加元素
                        break;
                    case List<int> intList:
                        var intListVal = intList.Aggregate("",
                            (current, item) => current + $"{item}&{(isToLower ? p.Name.FirstCharToLower() : p.Name)}[]=");

                        intListVal = intListVal[..^$"&{(isToLower ? p.Name.FirstCharToLower() : p.Name)}[]=".Length];

                        dictionary.Add($"{(isToLower ? p.Name.FirstCharToLower() : p.Name)}[]", intListVal); // 向字典添加元素
                        break;
                    default:
                        dictionary.Add(p.Name, m.Invoke(obj, new object[] { })?.ToString()); // 向字典添加元素
                        break;
                }
            }
            else
            {
                dictionary.Add(p.Name, m.Invoke(obj, new object[] { })?.ToString()); // 向字典添加元素
            }
        }

        return dictionary.ToQueryString(isToLower: isToLower);
    }

    /// <summary>
    /// 将一个Object对象转为 字典
    /// </summary>
    /// <param name="obj"><see cref="object"/></param>
    /// <returns><see cref="IDictionary{TKey,TValue}"/></returns>
    public static IDictionary<string, string> ToDictionary(this object obj)
    {
        var dictionary = new Dictionary<string, string>();

        var t = obj.GetType(); // 获取对象对应的类， 对应的类型

        var pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性

        foreach (var p in pi)
        {
            var m = p.GetGetMethod();

            if (m == null || !m.IsPublic)
                continue;
            // 进行判NULL处理
            if (m.Invoke(obj, new object[] { }) != null)
            {
                dictionary.Add(p.Name, m.Invoke(obj, new object[] { })?.ToString()); // 向字典添加元素
            }
        }

        return dictionary;
    }

    /// <summary>
    /// 尝试获取对象的数量
    /// </summary>
    /// <param name="obj"><see cref="object"/></param>
    /// <param name="count">数量</param>
    /// <returns><see cref="bool"/></returns>
    public static bool TryGetCount(this object obj, out int count)
    {
        // 处理可直接获取长度的类型
        switch (obj)
        {
            // 检查对象是否是字符类型
            case char:
                count = 1;
                return true;
            // 检查对象是否是字符串类型
            case string text:
                count = text.Length;
                return true;
            // 检查对象是否实现了 ICollection 接口
            case ICollection collection:
                count = collection.Count;
                return true;
        }

        // 反射查找是否存在 Count 属性
        var runtimeProperty = obj.GetType().GetRuntimeProperty("Count");

        // 反射获取 Count 属性值
        if (runtimeProperty is not null && runtimeProperty.CanRead && runtimeProperty.PropertyType == typeof(int))
        {
            count = (int) runtimeProperty.GetValue(obj)!;
            return true;
        }

        count = -1;
        return false;
    }

    /// <summary>
    /// 判断是否是富基元类型
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsRichPrimitive(this Type type)
    {
        // 处理元组类型
        if (type.IsValueTuple())
            return false;

        // 处理数组类型，基元数组类型也可以是基元类型
        if (type.IsArray)
            return type.GetElementType().IsRichPrimitive();

        // 基元类型或值类型或字符串类型
        if (type.IsPrimitive || type.IsValueType || type == typeof(string))
            return true;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return type.GenericTypeArguments[0].IsRichPrimitive();

        return false;
    }

    /// <summary>
    /// 判断是否是元组类型
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsValueTuple(this Type type)
    {
        return type.Namespace == "System" && type.Name.Contains("ValueTuple`");
    }

    /// <summary>
    /// 获取字段特性
    /// </summary>
    /// <param name="field"><see cref="FieldInfo"/></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetDescriptionValue<T>(this FieldInfo field) where T : Attribute
    {
        // 获取字段的指定特性，不包含继承中的特性
        var customAttributes = field.GetCustomAttributes(typeof(T), false);

        // 如果没有数据返回null
        return customAttributes.Length > 0 ? (T) customAttributes[0] : null;
    }

    /// <summary>
    /// 判断方法是否是异步
    /// </summary>
    /// <param name="method"><see cref="MethodInfo"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsAsync(this MethodInfo method)
    {
        return method.GetCustomAttribute<AsyncMethodBuilderAttribute>() != null ||
               method.ReturnType.ToString().StartsWith(typeof(Task).FullName);
    }

    /// <summary>
    /// 获取方法真实返回类型
    /// </summary>
    /// <param name="method"><see cref="MethodInfo"/></param>
    /// <returns><see cref="Type"/></returns>
    public static Type GetRealReturnType(this MethodInfo method)
    {
        // 判断是否是异步方法
        var isAsyncMethod = method.IsAsync();

        // 获取类型返回值并处理 Task 和 Task<T> 类型返回值
        var returnType = method.ReturnType;
        return isAsyncMethod ? (returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(void)) : returnType;
    }

    /// <summary>
    /// 查找方法指定特性，如果没找到则继续查找声明类
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="method"><see cref="MethodInfo"/></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static TAttribute GetFoundAttribute<TAttribute>(this MethodInfo method, bool inherit) where TAttribute : Attribute
    {
        // 获取方法所在类型
        var declaringType = method.DeclaringType;

        var attributeType = typeof(TAttribute);

        // 判断方法是否定义指定特性，如果没有再查找声明类
        var foundAttribute = method.IsDefined(attributeType, inherit)
            ? method.GetCustomAttribute<TAttribute>(inherit)
            : (declaringType != null && declaringType.IsDefined(attributeType, inherit)
                ? declaringType.GetCustomAttribute<TAttribute>(inherit)
                : default);

        return foundAttribute;
    }
}