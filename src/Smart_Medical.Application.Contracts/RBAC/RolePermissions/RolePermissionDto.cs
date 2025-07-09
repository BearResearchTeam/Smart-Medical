using System;
using Volo.Abp.Application.Dtos;
using Smart_Medical.RBAC.Roles;
using Smart_Medical.RBAC.Permissions;

namespace Smart_Medical.RBAC.RolePermissions
{
    public class RolePermissionDto : AuditedEntityDto<Guid>
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }

        // 导航属性的DTO，用于展示关联的角色和权限信息
        public RoleDto Role { get; set; }
        public PermissionDto Permission { get; set; }

    }
}