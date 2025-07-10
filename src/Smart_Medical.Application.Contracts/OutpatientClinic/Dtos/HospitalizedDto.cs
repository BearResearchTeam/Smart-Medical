using System;
using System.ComponentModel.DataAnnotations;

namespace Smart_Medical.OutpatientClinic.Dtos
{
    public class HospitalizedDto
    {
        public Guid Id { get; set; }
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
    }

    public class GetAllSickInfoDto
    {
        public Guid Id { get; set; }

        public string PatientName { get; set; }
    }

    public enum HospitalizationStatus
    {
        /// <summary>
        /// 已登记，尚未入院
        /// </summary>
        Registered = 0,

        /// <summary>
        /// 正在住院中
        /// </summary>
        InHospital = 1,

        /// <summary>
        /// 已出院
        /// </summary>
        Discharged = 2,

        /// <summary>
        /// 已结算
        /// </summary>
        Settled = 3,

        /// <summary>
        /// 转院中
        /// </summary>
        Transferring = 4,

        /// <summary>
        /// 自动出院（如患者擅自离院）
        /// </summary>
        AutoDischarged = 5,

        /// <summary>
        /// 死亡
        /// </summary>
        Deceased = 6
    }



}