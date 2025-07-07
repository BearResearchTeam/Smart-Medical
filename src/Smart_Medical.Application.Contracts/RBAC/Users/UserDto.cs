using Smart_Medical.RBAC.UserRoles;
using Smart_Medical.UserLoginECC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Smart_Medical.RBAC.Users
{
    /// <summary>
    /// 用户响应dto
    /// </summary>
    public class UserDto : ResultLoginDto
    {
        public Guid Id { get; set; }
        //public string UserName { get; set; }
        public string RoleName { get; set; }
        
        //public string UserEmail { get; set; }
        //public string UserPhone { get; set; }
        //public bool? UserSex { get; set; }
        public List<string> Permissions { get; set; }
        //public List<string> Roles { get; set; }

        // 导航属性的DTO，用于展示关联的用户角色信息
        public ICollection<UserRoleDto> UserRoles { get; set; }
    }
}
