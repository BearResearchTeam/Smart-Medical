using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.DoctorvVsit.DoctorAccounts
{
    public class DoctorAccountListDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// 所属科室编号
        /// </summary> /// <summary>
        /// 所属科室编号
        /// </summary>
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// 是否审核
        /// </summary>
        public int IsActive { get; set; } = 0;
        /// <summary>
        /// 医生个人头像
        /// </summary>
        public string Doctorimgs { get; set; } = string.Empty;

        /// <summary>
        /// 医生职称
        /// </summary>
        [Required(ErrorMessage = "账户标识不能为空")]
        [StringLength(200, ErrorMessage = "账户标识长度不能超过200个字符")]
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// 医生工号
        /// </summary>
        //[Required(ErrorMessage = "工号不能为空")]
        [StringLength(100, ErrorMessage = "工号长度不能超过100个字符")]
        public string EmployeeId { get; set; } = string.Empty;

        /// <summary>
        /// 医生姓名
        /// </summary>
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(20, ErrorMessage = "姓名长度不能超过20个字符")]
        public string EmployeeName { get; set; } = string.Empty;
        /// <summary>
        /// 医生手机号
        /// </summary>
        public string EmployeePhone { get; set; } = string.Empty;
        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// 所属机构名称
        /// </summary>
        [Required(ErrorMessage = "机构名称不能为空")]
        [StringLength(50, ErrorMessage = "机构名称长度不能超过50个字符")]
        public string InstitutionName { get; set; } = string.Empty;

        /// <summary>
        /// 所属科室名称
        /// </summary>
        [StringLength(30, ErrorMessage = "科室名称长度不能超过30个字符")]
        public string DepartmentName { get; set; } = string.Empty;
        /// <summary>
        /// 医生擅长
        /// </summary>
        public string DoctorGoodat { get; set; }

        /// <summary>
        /// 医生简介
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 医生证书
        /// </summary>
        public string Certificate { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string AuditName { get; set; }
    }

    /// <summary>
    /// 医生科室
    /// </summary>
    public class DoctorDepartDto
    {

        public Guid Id { get; set; }
        /// <summary>
        /// 科室名称（必填）
        /// </summary>
        [Required(ErrorMessage = "科室名称不能为空")]
        [StringLength(50, ErrorMessage = "科室名称长度不能超过50个字符")]
        public string DepartmentName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 医生列表
    /// </summary>
    public class DoctorListDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// 员工姓名
        /// </summary>
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(20, ErrorMessage = "姓名长度不能超过20个字符")]
        public string EmployeeName { get; set; } = string.Empty;
    }
}
