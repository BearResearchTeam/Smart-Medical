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
        // ���滻ΪRedis�ȷֲ�ʽ����
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
            // ��ѯ�û����н�ɫ
            var roleIds = (await _userRoleRepository.GetListAsync(x => x.UserId == userId)).Select(x => x.RoleId).ToList();
            // ��ѯ���н�ɫ��Ȩ��
            var permissionIds = (await _rolePermissionRepository.GetListAsync(x => roleIds.Contains(x.RoleId))).Select(x => x.PermissionId).Distinct().ToList();
            // ��ѯȨ�ޱ���
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
            // �ҵ�����ӵ�иý�ɫ���û��������仺��
            var userIds = (await _userRoleRepository.GetListAsync(x => x.RoleId == roleId)).Select(x => x.UserId).ToList();
            foreach (var userId in userIds)
            {
                _userPermissionCache.Remove(userId);
            }
        }
    }
}
