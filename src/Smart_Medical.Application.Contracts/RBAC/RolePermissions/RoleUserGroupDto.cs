using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.RBAC.RolePermissions
{
    public class RoleUserGroupDto
    {
        public string RoleName { get; set; }
        public List<UserSimpleDto> Users { get; set; }
    }

    public class UserSimpleDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
