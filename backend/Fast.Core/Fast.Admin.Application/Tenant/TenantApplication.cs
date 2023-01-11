﻿using Fast.Core.AdminFactory.ServiceFactory.Tenant;
using Fast.Core.AdminFactory.ServiceFactory.Tenant.Dto;
using Fast.Core.ServiceCollection.RequestLimit.AttributeFilter;
using Fast.Core.Util.Restful.Internal;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Application.Tenant;

/// <summary>
/// 租户接口
/// </summary>
[ApiDescriptionSettings(Name = "Tenant", Order = 100), RequestLimit(false)]
public class TenantApplication : IDynamicApiController
{
    private readonly ISysTenantService _sysTenantService;

    public TenantApplication(ISysTenantService sysTenantService)
    {
        _sysTenantService = sysTenantService;
    }

    /// <summary>
    /// Web站点初始化
    /// </summary>
    /// <returns></returns>
    [HttpGet("webSiteInit", "Web站点初始化"), AllowAnonymous, DisableOpLog]
    public async Task<WebSiteInitOutput> WebSiteInit()
    {
        return await _sysTenantService.WebSiteInit();
    }

    /// <summary>
    /// 分页查询租户信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("sysTenant/page", "分页查询租户信息")]
    public async Task<PageResult<TenantOutput>> QueryTenantPageList([FromQuery] QueryTenantInput input)
    {
        return await _sysTenantService.QueryTenantPageList(input);
    }

    /// <summary>
    /// 添加租户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("sysTenant/add", "添加租户")]
    public async Task AddTenant(AddTenantInput input)
    {
        await _sysTenantService.AddTenant(input);
    }

    /// <summary>
    /// 初始化租户信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("sysTenant/initTenantInfo", "初始化租户信息")]
    public async Task InitTenantInfo(InitTenantInfoInput input)
    {
        await _sysTenantService.InitTenantInfo(input);
    }
}