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
        /// �ѵǼǣ���δ��Ժ
        /// </summary>
        Registered = 0,

        /// <summary>
        /// ����סԺ��
        /// </summary>
        InHospital = 1,

        /// <summary>
        /// �ѳ�Ժ
        /// </summary>
        Discharged = 2,

        /// <summary>
        /// �ѽ���
        /// </summary>
        Settled = 3,

        /// <summary>
        /// תԺ��
        /// </summary>
        Transferring = 4,

        /// <summary>
        /// �Զ���Ժ���综��������Ժ��
        /// </summary>
        AutoDischarged = 5,

        /// <summary>
        /// ����
        /// </summary>
        Deceased = 6
    }



}