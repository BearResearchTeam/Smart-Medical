using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smart_Medical.Authorization
{
    /// <summary>
    /// 权限缓存管理服务，支持批量刷新/清理用户和角色的权限缓存
    /// </summary>
    public class PermissionCacheManagerService
    {
        private readonly IPermissionCacheService _permissionCacheService;

        /// <summary>
        /// 构造函数，注入权限缓存服务
        /// </summary>
        /// <param name="permissionCacheService">权限缓存服务实例</param>
        public PermissionCacheManagerService(IPermissionCacheService permissionCacheService)
        {
            _permissionCacheService = permissionCacheService;
        }

        /// <summary>
        /// 批量使一组用户的权限缓存失效（如用户角色或角色权限批量变更时调用）
        /// </summary>
        /// <param name="userIds">用户ID集合</param>
        public async Task InvalidateUsersAsync(IEnumerable<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                await _permissionCacheService.InvalidateUserPermissionsAsync(userId);
            }
        }

        /// <summary>
        /// 批量使一组角色下所有用户的权限缓存失效（如角色权限批量变更时调用）
        /// </summary>
        /// <param name="roleIds">角色ID集合</param>
        public async Task InvalidateRolesAsync(IEnumerable<Guid> roleIds)
        {
            foreach (var roleId in roleIds)
            {
                await _permissionCacheService.InvalidateRolePermissionsAsync(roleId);
            }
        }
    }
} 