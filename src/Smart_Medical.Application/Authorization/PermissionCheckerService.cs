using Smart_Medical.RBAC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Authorization
{
    /// <summary>
    /// 权限校验服务实现类，负责判断用户是否拥有指定权限及获取用户所有权限（集成缓存机制）
    /// </summary>
    public class PermissionCheckerService : IPermissionCheckerService
    {
        private readonly IRepository<UserRole, Guid> _userRoleRepository;
        private readonly IRepository<RolePermission, Guid> _rolePermissionRepository;
        private readonly IRepository<Permission, Guid> _permissionRepository;
        private readonly IPermissionCacheService _permissionCacheService;

        /// <summary>
        /// 构造函数，注入所需的仓储和缓存服务
        /// </summary>
        /// <param name="userRoleRepository">用户-角色仓储</param>
        /// <param name="rolePermissionRepository">角色-权限仓储</param>
        /// <param name="permissionRepository">权限仓储</param>
        /// <param name="permissionCacheService">权限缓存服务</param>
        public PermissionCheckerService(
            IRepository<UserRole, Guid> userRoleRepository,
            IRepository<RolePermission, Guid> rolePermissionRepository,
            IRepository<Permission, Guid> permissionRepository,
            IPermissionCacheService permissionCacheService)
        {
            _userRoleRepository = userRoleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
            _permissionCacheService = permissionCacheService;
        }

        /// <summary>
        /// 判断指定用户是否拥有某个权限（优先走缓存）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permissionCode">权限编码</param>
        /// <returns>如果拥有该权限返回true，否则返回false</returns>
        public async Task<bool> IsGrantedAsync(Guid userId, string permissionCode)
        {
            var permissions = await _permissionCacheService.GetUserPermissionsAsync(userId);
            return permissions.Contains(permissionCode);
        }

        /// <summary>
        /// 获取指定用户拥有的所有权限编码（优先走缓存）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>该用户拥有的权限编码列表</returns>
        public async Task<List<string>> GetGrantedPermissionsAsync(Guid userId)
        {
            return await _permissionCacheService.GetUserPermissionsAsync(userId);
        }
    }
}
