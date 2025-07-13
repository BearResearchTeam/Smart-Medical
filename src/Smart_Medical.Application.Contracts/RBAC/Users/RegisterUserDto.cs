using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.RBAC.Users
{
    public class RegisterUserDto
    {
        // 用户信息
        [Required, StringLength(50, MinimumLength = 2)]
        public string UserName { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public string UserPwd { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string UserEmail { get; set; }

        [Phone, StringLength(20)]
        public string UserPhone { get; set; }

        public bool? UserSex { get; set; }

        // 患者信息（可选扩展字段）
        [StringLength(18, MinimumLength = 18)]
        public string IdNumber { get; set; }

        [StringLength(20)]
        public string VisitType { get; set; } = "初诊";

        public bool IsInfectiousDisease { get; set; } = false;

        public DateTime? DiseaseOnsetTime { get; set; }

        public DateTime? EmergencyTime { get; set; }

        [StringLength(20)]
        public string VisitStatus { get; set; } = "待就诊";

        public DateTime VisitDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 年龄
        /// </summary>
        [Range(0, 150, ErrorMessage = "年龄必须在0-150之间")]
        public int? Age { get; set; }

        /// <summary>
        /// 年龄单位（年/月/日）
        /// </summary>
        [StringLength(10, ErrorMessage = "年龄单位长度不能超过10个字符")]
        public string AgeUnit { get; set; } = string.Empty;
    }
}
