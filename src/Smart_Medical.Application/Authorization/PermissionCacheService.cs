using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smart_Medical.RBAC;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Authorization
{
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly IRepository<UserRole, Guid> _userRoleRepository;
        private readonly IRepository<RolePermission, Guid> _rolePermissionRepository;
        private readonly IRepository<Permission, Guid> _permissionRepository;
        // 可替换为Redis等分布式缓存
        private static readonly Dictionary<Guid, List<string>> _userPermissionCache = new();

        public PermissionCacheService(
            IRepository<UserRole, Guid> userRoleRepository,
            IRepository<RolePermission, Guid> rolePermissionRepository,
            IRepository<Permission, Guid> permissionRepository)
        {
            _userRoleRepository = userRoleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {
            if (_userPermissionCache.TryGetValue(userId, out var cached))
                return cached;
            // 查询用户所有角色
            var roleIds = (await _userRoleRepository.GetListAsync(x => x.UserId == userId)).Select(x => x.RoleId).ToList();
            // 查询所有角色的权限
            var permissionIds = (await _rolePermissionRepository.GetListAsync(x => roleIds.Contains(x.RoleId))).Select(x => x.PermissionId).Distinct().ToList();
            // 查询权限编码
            var permissions = (await _permissionRepository.GetListAsync(x => permissionIds.Contains(x.Id))).Select(x => x.PermissionCode).ToList();
            _userPermissionCache[userId] = permissions;
            return permissions;
        }

        public Task InvalidateUserPermissionsAsync(Guid userId)
        {
            _userPermissionCache.Remove(userId);
            return Task.CompletedTask;
        }

        public async Task InvalidateRolePermissionsAsync(Guid roleId)
        {
            // 找到所有拥有该角色的用户，清理其缓存
            var userIds = (await _userRoleRepository.GetListAsync(x => x.RoleId == roleId)).Select(x => x.UserId).ToList();
            foreach (var userId in userIds)
            {
                _userPermissionCache.Remove(userId);
            }
        }
    }
}
