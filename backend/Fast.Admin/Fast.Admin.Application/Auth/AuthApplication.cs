﻿using Fast.Admin.Service.Auth;
using Fast.Admin.Service.Auth.Dto;
using Fast.Core.Const;
using Fast.SDK.Common.EnumFactory;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Application.Auth;

/// <summary>
/// 租户授权接口
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Web, Name = "Auth", Order = 100)]
public class AuthApplication : IDynamicApiController
{
    private readonly ITenAuthService _tenAuthService;

    public AuthApplication(ITenAuthService tenAuthService)
    {
        _tenAuthService = tenAuthService;
    }

    /// <summary>
    /// Web登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("/webLogin", "Web登录", HttpRequestActionEnum.Auth), AllowAnonymous, DisableOpLog]
    public async Task WebLogin(WebLoginInput input)
    {
        await _tenAuthService.WebLogin(input);
    }

    /// <summary>
    /// 获取登录用户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("/getLoginUser", "获取登录用户信息", HttpRequestActionEnum.Query)]
    public async Task<GetLoginUserInfoOutput> GetLoginUserInfo()
    {
        return await _tenAuthService.GetLoginUserInfo();
    }
}