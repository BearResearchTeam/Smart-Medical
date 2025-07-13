using System;
using System.Collections.Generic; // 引入集合类型命名空间，用于 ICollection
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Smart_Medical.RBAC
{
    /// <summary>
    /// 用户实体类
    /// 继承自 FullAuditedAggregateRoot<Guid>，表示这是一个具有完整审计功能（创建/修改/删除时间、操作人）的聚合根实体，主键类型为Guid。
    /// </summary>
    public class User : FullAuditedAggregateRoot<Guid>
    {

        [Required(ErrorMessage = "用户名是必填项。")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "用户名长度必须在 2 到 50 个字符之间。")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码是必填项。")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度至少为 6 个字符。")]
        [DataType(DataType.Password)] // 提示此字段包含密码数据，用于 UI 渲染
        public string UserPwd { get; set; }

        [Required(ErrorMessage = "电子邮件地址是必填项。")]
        [EmailAddress(ErrorMessage = "电子邮件地址格式不正确。")]
        [StringLength(100, ErrorMessage = "电子邮件地址不能超过 100 个字符。")]
        public string UserEmail { get; set; }

        [Phone(ErrorMessage = "电话号码格式不正确。")]
        [StringLength(20, ErrorMessage = "电话号码不能超过 20 个字符。")]
        public string UserPhone { get; set; }

        // 对于布尔属性，通常不使用 [Required]，除非您特别需要强制要求一个值 (true/false)
        // 而不是 null (如果它是可空布尔类型的话)。
        // 大多数情况下，布尔值总会有一个默认值 false。 默认是未知  后续可以修改性别 
        public bool? UserSex { get; set; } = null;

        /// <summary>
        /// 用户和角色之间的多对多关联导航属性。
        /// ICollection<UserRole> 表示一个用户可以拥有多个角色通过 UserRole 关联。
        /// 这是一个集合导航属性，用于从 User 实体导航到与之关联的所有 UserRole 实体。
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; } // 新增：用户和角色之间的关联

        /// <summary>
        /// 用户关联的患者集合
        /// </summary>
        public ICollection<UserPatient> UserPatients { get; set; }
    }
}
