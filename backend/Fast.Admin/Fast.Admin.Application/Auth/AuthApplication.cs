﻿using Fast.Admin.Service.Auth;
using Fast.Admin.Service.Auth.Dto;
using Fast.Core.AdminFactory.EnumFactory;
using Fast.Core.Const;
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
}