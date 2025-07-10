using System;
using Volo.Abp.Application.Dtos;

namespace Smart_Medical.Registration.Dtos
{
    public class GetRegistrationsInput : PagedAndSortedResultRequestDto
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? PatientId { get; set; }
        public string? Status { get; set; }
        public string? PatientName { get; set; }
    }
} 