﻿namespace Fast.Core;

/// <summary>
/// 通用上下文
/// </summary>
public static class GlobalContext
{
    /// <summary>
    /// 当前租户Id
    /// </summary>
    public static long TenantId => (User?.FindFirst(ClaimConst.TENANT_ID)?.Value).ParseToLong();

    /// <summary>
    /// 任务调度租户Id
    /// </summary>
    public static long ScheduledTenantId => HttpContext == null ? 0 : HttpContext.Request.Headers["TenantId"].ParseToLong();

    /// <summary>
    /// 支付回调租户Id
    /// </summary>
    public static long PayTenantId { get; set; }

    /// <summary>
    /// 当前用户Id
    /// </summary>
    public static long UserId => (User?.FindFirst(ClaimConst.CLAINM_USERID)?.Value).ParseToLong();

    /// <summary>
    /// 当前用户账号
    /// </summary>
    public static string UserAccount => (User?.FindFirst(ClaimConst.CLAINM_ACCOUNT)?.Value).ParseToString();

    /// <summary>
    /// 当前用户名称
    /// </summary>
    public static string UserName => (User?.FindFirst(ClaimConst.CLAINM_NAME)?.Value).ParseToString();

    /// <summary>
    /// 是否超级管理员
    /// </summary>
    public static bool IsSuperAdmin
    {
        get
        {
            if (User == null)
                return false;
            return User.FindFirst(ClaimConst.CLAINM_SUPERADMIN)?.Value == AdminTypeEnum.SuperAdmin.GetHashCode().ParseToString();
        }
    }

    /// <summary>
    /// 是否系统管理员
    /// </summary>
    public static bool IsSystemAdmin
    {
        get
        {
            if (User == null)
                return false;
            return User.FindFirst(ClaimConst.CLAINM_SUPERADMIN)?.Value == AdminTypeEnum.SystemAdmin.GetHashCode().ParseToString();
        }
    }

    ///// <summary>
    ///// 当前用户信息
    ///// </summary>
    //public static SysUserModel UserModelInfo =>
    //    GetService<ISqlSugarRepository<SysUserModel>>().FirstOrDefault(f => f.Id == UserId);

    ///// <summary>
    ///// 检测用户是否存在
    ///// </summary>
    ///// <param name="userId"></param>
    ///// <returns></returns>
    //public static async Task<SysUserModel> CheckUserAsync(long userId)
    //{
    //    var user = await GetService<ISqlSugarRepository<SysUserModel>>().FirstOrDefaultAsync(u => u.Id == userId);
    //    return user ?? throw Oops.Bah(ErrorCodeEnum.D1002);
    //}

    ///// <summary>
    ///// 获取用户员工信息
    ///// </summary>
    ///// <param name="userId"></param>
    ///// <returns></returns>
    //public static async Task<SysEmpModel> GetUserEmpInfo(long userId)
    //{
    //    var emp = await GetService<ISqlSugarRepository<SysEmpModel>>().FirstOrDefaultAsync(u => u.Id == userId);
    //    return emp ?? throw Oops.Bah(ErrorCodeEnum.D1002);
    //}

    /// <summary>
    /// 租户库Db Info
    /// </summary>
    public static SysTenantDataBaseModel TenantDbInfo { get; set; }

    /// <summary>
    /// 获取租户Id
    /// </summary>
    /// <param name="isThrow">是否抛出错误</param>
    /// <returns></returns>
    public static long GetTenantId(bool isThrow = true)
    {
        if (!TenantId.IsNullOrZero())
        {
            return TenantId;
        }

        if (!ScheduledTenantId.IsNullOrZero())
        {
            return ScheduledTenantId;
        }

        if (!PayTenantId.IsNullOrZero())
        {
            return PayTenantId;
        }

        if (isThrow)
            throw Oops.Oh(ErrorCode.TenantSysError);

        return 0;
    }
}

/// <summary>
/// 数据库配置
/// </summary>
public class ConnectionStringsOptions : IConfigurableOptions
{
    /// <summary>
    /// 连接Id
    /// </summary>
    public string DefaultConnectionId { get; set; }

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string DefaultConnection { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public DbType DefaultDbType { get; set; }
}

/// <summary>
/// 系统配置
/// </summary>
public class SystemSettingsOptions : IConfigurableOptions
{
    /// <summary>
    /// 最大请求Body Size
    /// </summary>
    public long MaxRequestBodySize { get; set; }
}

/// <summary>
/// 上传文件配置
/// </summary>
public class UploadFileOptions : IConfigurableOptions
{
    /// <summary>
    /// 头像
    /// </summary>
    public FileDescription Avatar { get; set; }

    /// <summary>
    /// 编辑器
    /// </summary>
    public FileDescription Editor { get; set; }

    /// <summary>
    /// 默认
    /// </summary>
    public FileDescription Default { get; set; }

    /// <summary>
    /// 文件参数
    /// </summary>
    public class FileDescription
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public long maxSize { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string[] contentType { get; set; }
    }
}