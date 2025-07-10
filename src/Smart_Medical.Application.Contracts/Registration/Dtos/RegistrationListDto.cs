using System;
using Volo.Abp.Application.Dtos;

namespace Smart_Medical.Registration.Dtos
{
    public class RegistrationListDto
    {
        public Guid Id { get; set; }
        public string DepartmentName { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string VisitType { get; set; }
        public string Status { get; set; }
        public string? Remarks { get; set; }

        // BasicPatientInfo fields
        public string VisitId { get; set; }
        public int Gender { get; set; }
        public int? Age { get; set; }
        public string AgeUnit { get; set; }
        public string ContactPhone { get; set; }
        public string IdNumber { get; set; }
        public bool IsInfectiousDisease { get; set; }
        public DateTime? DiseaseOnsetTime { get; set; }
        public DateTime? EmergencyTime { get; set; }
        public string PatientVisitStatus { get; set; }
        public DateTime PatientVisitDate { get; set; }
    }
} 