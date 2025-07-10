using Smart_Medical.Application.Contracts.Medical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Medical
{
    public class PatientMedicalInfoDto
    {
        // 就诊信息
        public Guid ClinicId { get; set; }
        public DateTime VisitDateTime { get; set; }
        public string DepartmentName { get; set; }
        public string ChiefComplaint { get; set; }
        public string PreliminaryDiagnosis { get; set; }
        public string VisitType { get; set; }
        public int ExecutionStatus { get; set; }
        public string ClinicRemarks { get; set; }

        // 病历信息
        public Guid SickId { get; set; }
        public string Status { get; set; }
        public decimal Temperature { get; set; }
        public int Pulse { get; set; }
        public int Breath { get; set; }
        public string BloodPressure { get; set; }
        public string AdmissionDiagnosis { get; set; }
        public string DischargeDiagnosis { get; set; }
        public string DischargeDepartment { get; set; }
        public DateTime? DischargeTime { get; set; }

        // 处方信息
        public Guid PrescriptionId { get; set; }
        public int PrescriptionTemplateNumber { get; set; }
        public string MedicalAdvice { get; set; }
        public string DrugIds { get; set; }
        public List<DrugItemDto> DrugItems { get; set; } = new List<DrugItemDto>();
    }
}
