using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.OutpatientClinic.Dtos
{
    /// <summary>
    /// 患者基本信息
    /// </summary>
    public class BasicPatientInfoDto
    {
        /// <summary>
        /// 患者姓名（必填）
        /// </summary>
        [Required(ErrorMessage = "患者姓名不能为空")]
        [StringLength(50, ErrorMessage = "患者姓名长度不能超过50个字符")]
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// 性别【1】男【2】女
        /// </summary>
        public int Gender { get; set; }

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

        /// <summary>
        /// 联系方式
        /// </summary>
        [StringLength(20, ErrorMessage = "联系方式长度不能超过20个字符")]
        [Phone(ErrorMessage = "请输入有效的电话号码")]
        public string ContactPhone { get; set; } = string.Empty;

        /// <summary>
        /// 身份证号
        /// </summary>
        [StringLength(18, ErrorMessage = "身份证号长度必须为18位", MinimumLength = 18)]
        public string IdNumber { get; set; } = string.Empty;

        /// <summary>
        /// 就诊类型（初诊/复诊，必填）
        /// </summary>
        [Required(ErrorMessage = "就诊类型不能为空")]
        [StringLength(20, ErrorMessage = "就诊类型长度不能超过20个字符")]
        public string VisitType { get; set; } = "初诊";

        /// <summary>
        /// 是否为传染病
        /// </summary>
        public bool IsInfectiousDisease { get; set; }

        /// <summary>
        /// 发病时间
        /// </summary>
        public DateTime? DiseaseOnsetTime { get; set; }        
    }

    
}
