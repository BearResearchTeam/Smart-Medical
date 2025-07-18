using Smart_Medical.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Smart_Medical.RBAC.Permissions
{
    public class PermissionDto : AuditedEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string PermissionName { get; set; }
        public string PermissionCode { get; set; }
        public PermissionType Type { get; set; }
        public string PagePath { get; set; }
        public Guid? ParentId { get; set; }
        public string Icon { get; set; }
    }


}