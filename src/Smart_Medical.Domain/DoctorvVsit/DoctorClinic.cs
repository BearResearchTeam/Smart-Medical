using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Smart_Medical.DoctorvVsit
{
    /// <summary>
    /// 就诊流程表
    /// </summary>
    public class DoctorClinic : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 患者ID 
        /// 关联到 BasicPatientInfo 实体，表示本次就诊的患者。
        /// </summary>
        [Required(ErrorMessage = "患者ID不能为空！")]
        public Guid PatientId { get; set; }

        /// <summary>
        /// 主治医生ID 
        /// 关联到 DoctorAccount 实体，表示本次就诊的主要负责医生。
        /// </summary>
        [Required(ErrorMessage = "主治医生ID不能为空！")]
        public Guid DoctorId { get; set; }

        /// <summary>
        /// 就诊日期和时间 
        /// 记录患者就诊的具体日期和时间。
        /// </summary>
        [Required(ErrorMessage = "就诊日期不能为空！")]
        public DateTime VisitDateTime { get; set; }

        /// <summary>
        /// 门诊科室名称 
        /// 患者就诊的科室名称，可用于快速展示，但建议通过 DoctorId 或 DepartmentId 关联 DoctorDepartment 获取完整信息。
        /// </summary>
        [Required(ErrorMessage = "门诊科室不能为空！")]
        [StringLength(50, ErrorMessage = "门诊科室名称不能超过50个字符！")]
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// 主诉 
        /// 患者本次就诊的主要症状或不适的简要描述。
        /// </summary>
        [StringLength(500, ErrorMessage = "主诉内容不能超过500个字符！")]
        public string? ChiefComplaint { get; set; } = string.Empty;

        /// <summary>
        /// 初步诊断 🩺
        /// 医生对患者病情做出的初步判断和诊断结果。
        /// </summary>
        [StringLength(1000, ErrorMessage = "初步诊断内容不能超过1000个字符！")]
        public string? PreliminaryDiagnosis { get; set; } = string.Empty;

        /// <summary>
        /// 就诊类型 
        /// 例如： 初诊、复诊、急诊等，建议使用枚举或常量进行规范。
        /// </summary>
        [Required(ErrorMessage = "就诊类型不能为空！")]
        [StringLength(20, ErrorMessage = "就诊类型长度不能超过20个字符！")]
        public string VisitType { get; set; } = "初诊"; // 默认值可以是你常用的类型

        /// <summary>
        /// // 发药状态，【0】未发药，【1】已发药，【2】已退药
        /// </summary>
        public int DispensingStatus { get; set; } = 0;

        /// <summary>
        /// 患者就诊状态
        /// </summary>
        public ExecutionStatus ExecutionStatus { get; set; } = ExecutionStatus.PendingConsultation;//默认状态为待就诊


        /// <summary>
        /// 备注信息 
        /// 任何需要补充的就诊相关说明或特殊情况。
        /// </summary>
        [StringLength(1000, ErrorMessage = "备注信息不能超过1000个字符！")]
        public string? Remarks { get; set; } = string.Empty;
    }

    /// <summary>
    /// 就诊状态 
    /// 【1】待就诊【2】已取消【3】已就诊【4】待评价
    /// </summary>
    public enum ExecutionStatus
    {
        //枚举【1】待审核【2】已退回【3】已撤回【4】待接诊【5】已取消【6】已街镇【7】待随访【8】待评价
        PendingConsultation = 1, // 待就诊
        Cancelled = 2, // 已取消
        Completed = 3, // 已就诊
        PendingEvaluation = 4// 待评价
        //PendingReview = 1, // 待审核
        // Returned = 2, // 已退回
        //Withdrawn = 3, // 已撤回
        //PendingFollowUp = 7, // 待随访
    }



}
