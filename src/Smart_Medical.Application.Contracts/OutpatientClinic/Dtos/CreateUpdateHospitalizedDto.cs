using System;
using System.ComponentModel.DataAnnotations;

namespace Smart_Medical.OutpatientClinic.Dtos
{
    public class CreateUpdateHospitalizedDto
    {
        [Required]
        public Guid BasicPatientId { get; set; }
        [Required]
        [StringLength(32)]
        public string Status { get; set; }
        [Required]
        [StringLength(50)]
        public string PatientName { get; set; } = string.Empty;
        [Required]
        [Range(30, 45)]
        public decimal Temperature { get; set; }
        [Required]
        [Range(20, 200)]
        public int Pulse { get; set; }
        [Required]
        [Range(5, 60)]
        public int Breath { get; set; }
        [Required]
        [StringLength(16)]
        public string BloodPressure { get; set; }
        [StringLength(128)]
        public string? DischargeDiagnosis { get; set; } = string.Empty;
        [StringLength(32)]
        public string? InpatientNumber { get; set; } = string.Empty;
        [StringLength(64)]
        public string? DischargeDepartment { get; set; } = string.Empty;
        public DateTime? DischargeTime { get; set; } = null;
        [StringLength(128)]
        public string? AdmissionDiagnosis { get; set; } = string.Empty;
    }
} 