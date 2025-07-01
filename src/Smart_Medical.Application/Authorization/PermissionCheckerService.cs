using Smart_Medical.RBAC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Authorization
{
    /// <summary>
    /// Ȩ��У�����ʵ���࣬�����ж��û��Ƿ�ӵ��ָ��Ȩ�޼���ȡ�û�����Ȩ�ޣ����ɻ�����ƣ�
    /// </summary>
    public class PermissionCheckerService : IPermissionCheckerService
    {
        private readonly IRepository<UserRole, Guid> _userRoleRepository;
        private readonly IRepository<RolePermission, Guid> _rolePermissionRepository;
        private readonly IRepository<Permission, Guid> _permissionRepository;
        private readonly IPermissionCacheService _permissionCacheService;

        /// <summary>
        /// ���캯����ע������Ĳִ��ͻ������
        /// </summary>
        /// <param name="userRoleRepository">�û�-��ɫ�ִ�</param>
        /// <param name="rolePermissionRepository">��ɫ-Ȩ�޲ִ�</param>
        /// <param name="permissionRepository">Ȩ�޲ִ�</param>
        /// <param name="permissionCacheService">Ȩ�޻������</param>
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
        /// �ж�ָ���û��Ƿ�ӵ��ĳ��Ȩ�ޣ������߻��棩
        /// </summary>
        /// <param name="userId">�û�ID</param>
        /// <param name="permissionCode">Ȩ�ޱ���</param>
        /// <returns>���ӵ�и�Ȩ�޷���true�����򷵻�false</returns>
        public async Task<bool> IsGrantedAsync(Guid userId, string permissionCode)
        {
            var permissions = await _permissionCacheService.GetUserPermissionsAsync(userId);
            return permissions.Contains(permissionCode);
        }

        /// <summary>
        /// ��ȡָ���û�ӵ�е�����Ȩ�ޱ��루�����߻��棩
        /// </summary>
        /// <param name="userId">�û�ID</param>
        /// <returns>���û�ӵ�е�Ȩ�ޱ����б�</returns>
        public async Task<List<string>> GetGrantedPermissionsAsync(Guid userId)
        {
            return await _permissionCacheService.GetUserPermissionsAsync(userId);
        }
    }
}
