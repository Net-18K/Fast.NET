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

using Fast.Gateway.Entities.Entities.Projects;
using Fast.Gateway.Service.Projects.Dto;
using Mapster;

namespace Fast.Gateway.Service.Projects;

/// <summary>
/// <see cref="ProjectService"/> 项目服务
/// </summary>
public class ProjectService
{
    private readonly ISqlSugarRepository<Project> _repository;

    public ProjectService(ISqlSugarRepository<Project> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 分页获取项目下拉框
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<SqlSugarPageResult<ElementSelectorOutput>> QueryProjectSelector(SqlSugarPageInput input)
    {
        return await _repository.AsQueryable().OrderBy(ob => ob.ProjectName)
            .Select(sl => new ElementSelectorOutput {Label = sl.ProjectName, Value = sl.Id}).ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取项目分页
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<SqlSugarPageResult<QueryProjectPageOutput>> QueryProjectPage(QueryProjectPageInput input)
    {
        return await _repository.AsQueryable()
            .WhereIF(!input.SearchValue.IsEmpty(), wh => wh.ProjectName.Contains(input.SearchValue))
            .WhereIF(input.Enabled != null, wh => wh.Enabled == input.Enabled).Select<QueryProjectPageOutput>()
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    public async Task AddProject(AddProjectInput input)
    {
        // 判断项目名称是否重复
        if (await _repository.IsExistsAsync(wh => wh.ProjectName == input.ProjectName))
        {
            throw new UserFriendlyException($"项目名称【{input.ProjectName}】重复！");
        }

        var model = input.Adapt<Project>();
        // 默认启用
        model.Enabled = true;
        await _repository.InsertAsync(model);
    }
}