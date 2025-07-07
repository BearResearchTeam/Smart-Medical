using System;
using System.Collections.Generic;
using System.Threading.Tasks;


/// <summary>
/// 权限缓存服务接口，负责用户和角色的权限缓存获取与失效处理
/// </summary>
public interface IPermissionCacheService
{
    /// <summary>
    /// 获取指定用户的所有权限编码（通常带缓存）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>该用户拥有的权限编码列表</returns>
    Task<List<string>> GetUserPermissionsAsync(Guid userId);

    /// <summary>
    /// 使指定用户的权限缓存失效（如用户角色或角色权限变更时调用）
    /// </summary>
    /// <param name="userId">用户ID</param>
    Task InvalidateUserPermissionsAsync(Guid userId);

    /// <summary>
    /// 使指定角色下所有用户的权限缓存失效（如角色权限变更时调用）
    /// </summary>
    /// <param name="roleId">角色ID</param>
    Task InvalidateRolePermissionsAsync(Guid roleId);
}
