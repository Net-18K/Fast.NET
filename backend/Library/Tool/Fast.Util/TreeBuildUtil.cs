﻿using System.Collections;

namespace Fast.Util;

/// <summary>
/// 树基类
/// </summary>
public interface ITreeNode
{
    /// <summary>
    /// 获取节点id
    /// </summary>
    /// <returns></returns>
    long GetId();

    /// <summary>
    /// 获取节点父id
    /// </summary>
    /// <returns></returns>
    long GetPid();

    /// <summary>
    /// 获取排序字段
    /// </summary>
    /// <returns></returns>
    int Sort();

    /// <summary>
    /// 设置Children
    /// </summary>
    /// <param name="children"></param>
    void SetChildren(IList children);
}

/// <summary>
/// 递归工具类，用于遍历有父子关系的节点，例如菜单树，字典树等等
/// </summary>
/// <typeparam name="T"></typeparam>
public class TreeBuildUtil<T> where T : ITreeNode
{
    /// <summary>
    /// 顶级节点的父节点Id(默认0)
    /// </summary>
    // ReSharper disable once RedundantDefaultMemberInitializer
    private long _rootParentId = 0L;

    /// <summary>
    /// 设置根节点方法
    /// 查询数据可以设置其他节点为根节点，避免父节点永远是0，查询不到数据的问题
    /// </summary>
    public void SetRootParentId(long rootParentId)
    {
        _rootParentId = rootParentId;
    }

    /// <summary>
    /// 构造树节点
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns></returns>
    public List<T> Build(List<T> nodes)
    {
        var result = nodes.Where(i => i.GetPid() == _rootParentId).OrderBy(ob => ob.Sort()).ToList();
        result.ForEach(u => BuildChildNodes(nodes, u));
        return result;
    }

    /// <summary>
    /// 构造子节点集合
    /// </summary>
    /// <param name="totalNodes"></param>
    /// <param name="node"></param>
    private void BuildChildNodes(List<T> totalNodes, T node)
    {
        var nodeSubList = totalNodes.Where(i => i.GetPid() == node.GetId()).OrderBy(ob => ob.Sort()).ToList();
        nodeSubList.ForEach(u => BuildChildNodes(totalNodes, u));
        node.SetChildren(nodeSubList);
    }
}