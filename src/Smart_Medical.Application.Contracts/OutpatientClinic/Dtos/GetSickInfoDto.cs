using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.OutpatientClinic.Dtos
{
    /// <summary>
    /// 患者所有病历信息
    /// </summary>
    public class GetSickInfoDto
    {
        public Guid BasicPatientId { get; set; }
        /// <summary>
        /// 体温（℃）【BasicPatientInfo】
        /// </summary>
        [Required]
        [Range(30, 45, ErrorMessage = "体温应在30~45℃之间")]
        public decimal Temperature { get; set; }

        /// <summary>
        /// 脉搏（次/min）【BasicPatientInfo】
        /// </summary>
        [Required]
        [Range(20, 200, ErrorMessage = "脉搏应在20~200次/min之间")]
        public int Pulse { get; set; }

        /// <summary>
        /// 呼吸（次/min）【BasicPatientInfo】
        /// </summary>
        [Required]
        [Range(5, 60, ErrorMessage = "呼吸应在5~60次/min之间")]
        public int Breath { get; set; }

        /// <summary>
        /// 血压（mmHg）【BasicPatientInfo】
        /// </summary>
        [Required]
        [StringLength(16)]
        public string BloodPressure { get; set; }

        /// <summary>
        /// 主诉 【DoctorClinic】
        /// 患者本次就诊的主要症状或不适的简要描述。
        /// </summary>
        [StringLength(500, ErrorMessage = "主诉内容不能超过500个字符！")]
        public string? ChiefComplaint { get; set; }

        /// <summary>
        /// 不使用处方模版时，  药品ID集合【PatientPrescription】
        /// </summary>
        public string? DrugIds { get; set; } = string.Empty;

        /// <summary>
        /// 处方模板编号0【无模板（非模板录入）】1【西药处方】2【中药处方】【PatientPrescription】
        /// </summary>
        public int PrescriptionTemplateNumber { get; set; } = 0;

        /// <summary>
        /// 医嘱内容【PatientPrescription】
        /// </summary>
        [StringLength(200, ErrorMessage = "医嘱内容长度不能超过200个字符")]
        public string MedicalAdvice { get; set; } = string.Empty;

        /// <summary>
        /// 药物处方
        /// </summary>
        public List<DrugItemDto> DrugItems { get; set; }
    }

    public class DrugItemDto
    {
        /// <summary>
        /// 药品ID（关联 Drug.Id）
        /// </summary>
        public int DrugId { get; set; }

        public string DrugName { get; set; }

        /// <summary>
        /// 单次用药剂量（如 0.5、1.0 等）
        /// </summary>
        public decimal Dosage { get; set; }

        /// <summary>
        /// 剂量单位（如：片、ml、mg）
        /// </summary>
        public string DosageUnit { get; set; }

        /// <summary>
        /// 用法（如：口服、肌注、静滴等）
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// 用药频率（如：每日一次、每日三次、睡前服用等）
        /// </summary>
        public string Frequency { get; set; }

        /// <summary>
        /// 开药总数量（如开30片）
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 数量单位（如：片、瓶、支）
        /// </summary>
        public string NumberUnit { get; set; }

        /// <summary>
        /// 医嘱内容（医生额外说明，可选）
        /// </summary>
        public string? MedicalAdvice { get; set; }
    }
}
