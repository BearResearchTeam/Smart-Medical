using System;
using System.Collections.Generic;

namespace Smart_Medical.Application.Contracts.Medical
{
    /// <summary>
    /// 病历管理全信息DTO（包含病历、患者、就诊、处方、药品、预约）
    /// </summary>
    public class SickFullInfoDto
    {
        // 病历信息
        public Guid SickId { get; set; }
        public Guid BasicPatientId { get; set; }
        public string Status { get; set; }
        public string PatientName { get; set; }
        public decimal Temperature { get; set; }
        public int Pulse { get; set; }
        public int Breath { get; set; }
        public string BloodPressure { get; set; }
        public string? DischargeDiagnosis { get; set; }
        public string? InpatientNumber { get; set; }
        public string? DischargeDepartment { get; set; }
        public DateTime? DischargeTime { get; set; }
        public string? AdmissionDiagnosis { get; set; }

        // 患者基本信息
        public string PatientBaseName { get; set; }
        public int Gender { get; set; }
        public int? Age { get; set; }
        public string AgeUnit { get; set; }
        public string ContactPhone { get; set; }
        public string IdNumber { get; set; }
        public string VisitType { get; set; }
        public bool IsInfectiousDisease { get; set; }
        public DateTime? DiseaseOnsetTime { get; set; }
        public DateTime? EmergencyTime { get; set; }
        public string VisitStatus { get; set; }
        public DateTime VisitDate { get; set; }

        // 就诊信息
        public Guid ClinicId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime VisitDateTime { get; set; }
        public string DepartmentName { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? PreliminaryDiagnosis { get; set; }
        public string VisitTypeClinic { get; set; }
        public int DispensingStatus { get; set; }
        public int ExecutionStatus { get; set; }
        public string? ClinicRemarks { get; set; }

        // 处方信息
        public Guid PrescriptionId { get; set; }
        public int PrescriptionTemplateNumber { get; set; }
        public string? MedicalAdvice { get; set; }
        public List<DrugItemDto> DrugItems { get; set; } = new List<DrugItemDto>();

        // 药品信息（明细见 DrugItems）

        // 预约信息
        public Guid AppointmentId { get; set; }
        public DateTime? AppointmentDateTime { get; set; }
        public int AppointmentStatus { get; set; }
        public decimal ActualFee { get; set; }
        public string AppointmentRemarks { get; set; }
    }

    /// <summary>
    /// 药品明细DTO
    /// </summary>
    public class DrugItemDto
    {
        public int DrugId { get; set; }
        public string DrugName { get; set; }
        public string Specification { get; set; }
        // 可扩展更多药品字段
    }
} 