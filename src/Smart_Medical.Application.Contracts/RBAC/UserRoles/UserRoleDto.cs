using System;
using Volo.Abp.Application.Dtos;
using Smart_Medical.RBAC.Users;
using Smart_Medical.RBAC.Roles;

namespace Smart_Medical.RBAC.UserRoles
{
    public class UserRoleDto : AuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        // 导航属性的DTO，用于展示关联的用户和角色信息
        public UserDto User { get; set; }
        public RoleDto Role { get; set; }
    }
}