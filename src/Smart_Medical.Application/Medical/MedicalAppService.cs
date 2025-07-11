

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Smart_Medical.Application.Contracts.Medical;
using Smart_Medical.DoctorvVsit;
using Smart_Medical.Medical.Smart_Medical.Medical;
using Smart_Medical.OutpatientClinic.Dtos.Parameter;
using Smart_Medical.Patient;
using Smart_Medical.Pharmacy;
using Smart_Medical.Pharmacy.InAndOutWarehouse;
using Smart_Medical.RBAC;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Smart_Medical.Medical
{
    /// <summary>
    /// 病历管理AppService
    /// </summary>
    [ApiExplorerSettings(GroupName = "病种管理")]
    public class MedicalAppService : ApplicationService
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        /// <summary>
        /// 就诊流程
        /// </summary>
        private readonly IRepository<DoctorClinic, Guid> _doctorclinRepo;
        /// <summary>
        /// 患者基本信息
        /// </summary>
        private readonly IRepository<BasicPatientInfo, Guid> _patientRepo;
        /// <summary>
        /// 患者病历信息
        /// </summary>
        private readonly IRepository<Sick, Guid> _sickRepo;
        /// <summary>
        /// 患者开具处方
        /// </summary>
        private readonly IRepository<PatientPrescription, Guid> _prescriptionRepo;
        /// <summary>
        /// 药品
        /// </summary>
        private readonly IRepository<Drug, int> _drugRepo;
        /// <summary>
        /// 预约记录
        /// </summary>
        private readonly IRepository<Patient.Appointment, Guid> _appointment;
        private readonly IRepository<UserPatient, Guid> _userPatientRepo;
        private readonly IRepository<DrugInStock, Guid> _drugInStockRepo;
        private readonly IRepository<MedicalHistory, Guid> _commpany;

        public MedicalAppService(IUnitOfWorkManager unitOfWorkManager, IRepository<DoctorClinic, Guid> doctorclinRepo, IRepository<BasicPatientInfo, Guid> patientRepo, IRepository<Sick, Guid> sickRepo, IRepository<PatientPrescription, Guid> prescriptionRepo, IRepository<Drug, int> drugRepo, IRepository<Patient.Appointment, Guid> appointment, IRepository<UserPatient, Guid> userPatientRepo, IRepository<DrugInStock, Guid> drugInStockRepo, IRepository<MedicalHistory, Guid> commpany)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _doctorclinRepo = doctorclinRepo;
            _patientRepo = patientRepo;
            _sickRepo = sickRepo;
            _prescriptionRepo = prescriptionRepo;
            _drugRepo = drugRepo;
            _appointment = appointment;
            _userPatientRepo = userPatientRepo;
            _drugInStockRepo = drugInStockRepo;
            _commpany = commpany;
        }

        public async Task<ApiResult<List<SickFullInfoDto>>> GetPatientSickFullInfoAsync()
        {
            // 1. 获取所有表的 IQueryable
            var sicks = await _sickRepo.GetQueryableAsync();
            var patients = await _patientRepo.GetQueryableAsync();
            var clinics = await _doctorclinRepo.GetQueryableAsync();
            var prescriptions = await _prescriptionRepo.GetQueryableAsync();
            var drugs = await _drugRepo.GetQueryableAsync();
            var appointments = await _appointment.GetQueryableAsync();

            // 2. 多表联查（不再按 patientId 过滤）
            var query = from sick in sicks
                        join patient in patients on sick.BasicPatientId equals patient.Id into patientGroup
                        from patient in patientGroup.DefaultIfEmpty()
                        join clinic in clinics on sick.BasicPatientId equals clinic.PatientId into clinicGroup
                        from clinic in clinicGroup.DefaultIfEmpty()
                        join prescription in prescriptions on sick.BasicPatientId equals prescription.PatientNumber into presGroup
                        from prescription in presGroup.DefaultIfEmpty()
                        join appointment in appointments on sick.BasicPatientId equals appointment.PatientId into appGroup
                        from appointment in appGroup.DefaultIfEmpty()
                        select new SickFullInfoDto
                        {
                            // 病历信息
                            SickId = sick.Id,
                            BasicPatientId = sick.BasicPatientId,
                            Status = sick.Status,
                            PatientName = sick.PatientName,
                            Temperature = sick.Temperature,
                            Pulse = sick.Pulse,
                            Breath = sick.Breath,
                            BloodPressure = sick.BloodPressure,
                            DischargeDiagnosis = sick.DischargeDiagnosis,
                            InpatientNumber = sick.InpatientNumber,
                            DischargeDepartment = sick.DischargeDepartment,
                            DischargeTime = sick.DischargeTime,
                            AdmissionDiagnosis = sick.AdmissionDiagnosis,

                            // 患者基本信息
                            PatientBaseName = patient == null ? null : patient.PatientName,
                            Gender = patient == null ? 0 : patient.Gender,
                            Age = patient == null ? 0 : patient.Age,
                            AgeUnit = patient == null ? null : patient.AgeUnit,
                            ContactPhone = patient == null ? null : patient.ContactPhone,
                            IdNumber = patient == null ? null : patient.IdNumber,
                            VisitType = patient == null ? null : patient.VisitType,
                            IsInfectiousDisease = patient == null ? false : patient.IsInfectiousDisease,
                            DiseaseOnsetTime = patient == null ? null : patient.DiseaseOnsetTime,
                            EmergencyTime = patient == null ? null : patient.EmergencyTime,
                            VisitStatus = patient == null ? null : patient.VisitStatus,
                            VisitDate = patient == null ? default(DateTime) : patient.VisitDate,

                            // 就诊信息
                            ClinicId = clinic != null ? clinic.Id : Guid.Empty,
                            DoctorId = clinic == null ? Guid.Empty : clinic.DoctorId,
                            VisitDateTime = clinic == null ? default(DateTime) : clinic.VisitDateTime,
                            DepartmentName = clinic == null ? null : clinic.DepartmentName,
                            ChiefComplaint = clinic == null ? null : clinic.ChiefComplaint,
                            PreliminaryDiagnosis = clinic == null ? null : clinic.PreliminaryDiagnosis,
                            VisitTypeClinic = clinic == null ? null : clinic.VisitType,
                            DispensingStatus = clinic == null ? 0 : clinic.DispensingStatus,
                            ExecutionStatus = clinic == null ? 0 : (clinic.ExecutionStatus != null ? (int)clinic.ExecutionStatus : 0),
                            ClinicRemarks = clinic == null ? null : clinic.Remarks,

                            // 处方信息
                            PrescriptionId = prescription != null ? prescription.Id : Guid.Empty,
                            PrescriptionTemplateNumber = prescription == null ? 0 : prescription.PrescriptionTemplateNumber,
                            MedicalAdvice = prescription == null ? null : prescription.MedicalAdvice,
                            DrugItems = prescription == null || string.IsNullOrEmpty(prescription.DrugIds)
                                ? new List<DrugItemDto>()
                                : Newtonsoft.Json.JsonConvert.DeserializeObject<List<DrugItemDto>>(prescription.DrugIds),

                            // 预约信息
                            AppointmentId = appointment != null ? appointment.Id : Guid.Empty,
                            AppointmentDateTime = appointment == null ? default(DateTime) : appointment.AppointmentDateTime,
                            AppointmentStatus = appointment == null ? 0 : (appointment.Status != null ? (int)appointment.Status : 0),
                            ActualFee = appointment == null ? 0 : appointment.ActualFee,
                            AppointmentRemarks = appointment == null ? null : appointment.Remarks
                        };

            var result = query.ToList();

            // 3. 补全药品明细的药品名称等信息
            foreach (var dto in result)
            {
                foreach (var drug in dto.DrugItems)
                {
                    var drugInfo = drugs.FirstOrDefault(d => d.Id == drug.DrugId);
                    if (drugInfo != null)
                    {
                        drug.DrugName = drugInfo.DrugName;
                        drug.Specification = drugInfo.Specification;
                        // ...补全其他药品字段
                    }
                }
            }

            return ApiResult<List<SickFullInfoDto>>.Success(result, ResultCode.Success);
        }


        /// <summary>
        /// 病历信息导出
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public FileResult ExportSickExcel()
        {
            var list = await _repository.GetQueryableAsync();
            list = list.WhereIf(!string.IsNullOrWhiteSpace(search.PatientName), x => x.PatientName.Contains(search.PatientName))
                       .WhereIf(!string.IsNullOrWhiteSpace(search.InpatientNumber), x => x.InpatientNumber.Contains(search.InpatientNumber))
                       .WhereIf(!string.IsNullOrWhiteSpace(search.AdmissionDiagnosis), x => x.AdmissionDiagnosis.Contains(search.AdmissionDiagnosis));
            // 1. 获取病历信息数据
            var sickList = (from sick in _sickRepo.GetListAsync().Result
                            join patient in _patientRepo.GetListAsync().Result on sick.BasicPatientId equals patient.Id into sj
                            from patient in sj.DefaultIfEmpty()
                            select new SickFullInfoDto
                            {
                                SickId = sick.Id,
                                PatientName = patient != null ? patient.PatientName : string.Empty,
                                Gender = patient != null ? patient.Gender : 0,
                                Age = patient != null ? patient.Age : 0,
                                AdmissionDiagnosis = sick.AdmissionDiagnosis,
                                DischargeDiagnosis = sick.DischargeDiagnosis,
                                DischargeTime = sick.DischargeTime,
                                // ... 其他字段 ...
                            }).ToList();

            // 2. 创建Excel表
            IWorkbook workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("病历信息表");

          

            // 3. 表头
            var row0 = sheet.CreateRow(0);
            row0.CreateCell(0).SetCellValue("病历ID");
            row0.CreateCell(1).SetCellValue("患者ID");
            row0.CreateCell(2).SetCellValue("患者姓名");
            row0.CreateCell(3).SetCellValue("性别");
            row0.CreateCell(4).SetCellValue("年龄");
            row0.CreateCell(5).SetCellValue("年龄单位");
            row0.CreateCell(6).SetCellValue("联系电话");
            row0.CreateCell(7).SetCellValue("证件号");
            row0.CreateCell(8).SetCellValue("就诊类型");
            row0.CreateCell(9).SetCellValue("是否传染病");
            row0.CreateCell(10).SetCellValue("发病时间");
            row0.CreateCell(11).SetCellValue("急诊时间");
            row0.CreateCell(12).SetCellValue("就诊状态");
            row0.CreateCell(13).SetCellValue("就诊日期");
            row0.CreateCell(14).SetCellValue("入院诊断");
            row0.CreateCell(15).SetCellValue("出院诊断");
            row0.CreateCell(16).SetCellValue("入院号");
            row0.CreateCell(17).SetCellValue("出院科室");
            row0.CreateCell(18).SetCellValue("出院时间");
            row0.CreateCell(19).SetCellValue("主诉");
            row0.CreateCell(20).SetCellValue("初步诊断");
            row0.CreateCell(21).SetCellValue("医嘱");
            row0.CreateCell(22).SetCellValue("药品明细");
            row0.CreateCell(23).SetCellValue("预约ID");
            row0.CreateCell(24).SetCellValue("预约时间");
            row0.CreateCell(25).SetCellValue("预约状态");
            row0.CreateCell(26).SetCellValue("实收费用");
            row0.CreateCell(27).SetCellValue("预约备注");

            // 4. 填充数据
            int indexnum = 1;
            foreach (var item in sickList)
            {
                var row = sheet.CreateRow(indexnum);
                row.CreateCell(0).SetCellValue(item.SickId.ToString());
                row.CreateCell(1).SetCellValue(item.BasicPatientId.ToString());
                row.CreateCell(2).SetCellValue(item.PatientBaseName);
                row.CreateCell(3).SetCellValue(item.Gender == 1 ? "男" : "女");
                row.CreateCell(4).SetCellValue((double)item.Age);
                row.CreateCell(5).SetCellValue(item.AgeUnit);
                row.CreateCell(6).SetCellValue(item.ContactPhone);
                row.CreateCell(7).SetCellValue(item.IdNumber);
                row.CreateCell(8).SetCellValue(item.VisitType);
                row.CreateCell(9).SetCellValue(item.IsInfectiousDisease ? "是" : "否");
                row.CreateCell(10).SetCellValue(item.DiseaseOnsetTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                row.CreateCell(11).SetCellValue(item.EmergencyTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                row.CreateCell(12).SetCellValue(item.VisitStatus);
                row.CreateCell(13).SetCellValue(item.VisitDate.ToString("yyyy-MM-dd") ?? "");
                row.CreateCell(14).SetCellValue(item.AdmissionDiagnosis);
                row.CreateCell(15).SetCellValue(item.DischargeDiagnosis);
                row.CreateCell(16).SetCellValue(item.InpatientNumber);
                row.CreateCell(17).SetCellValue(item.DischargeDepartment);
                row.CreateCell(18).SetCellValue(item.DischargeTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                row.CreateCell(19).SetCellValue(item.ChiefComplaint);
                row.CreateCell(20).SetCellValue(item.PreliminaryDiagnosis);
                row.CreateCell(21).SetCellValue(item.MedicalAdvice);
                // 药品明细可拼接字符串
                row.CreateCell(22).SetCellValue(
                item.DrugItems != null
                    ? string.Join(";", item.DrugItems.Select(d => d.DrugName + "x" + d.DrugName))
                    : ""
            );

                row.CreateCell(23).SetCellValue(item.AppointmentId.ToString());
         
      
                row.CreateCell(24).SetCellValue(item.AppointmentDateTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                row.CreateCell(25).SetCellValue(item.AppointmentStatus.ToString());
                row.CreateCell(26).SetCellValue(item.ActualFee.ToString());
                row.CreateCell(27).SetCellValue(item.AppointmentRemarks);
                indexnum++;
            }


            // 5. 导出为字节流
            byte[] s;
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                s = ms.ToArray();
            }

            // 6. 返回文件
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return new FileContentResult(s, contentType)
            {
                FileDownloadName="病历信息.xlsx"
            };
        }

        /// <summary>
        /// 获取药品入库+药品+制药公司联合信息列表
        /// </summary>
       public async Task<ApiResult<List<DrugInStockCompanyFullDto>>> GetDrugInStockCompanyFullListAsync()
{
    var inStocks = await _drugInStockRepo.GetQueryableAsync();
    var drugs = await _drugRepo.GetQueryableAsync();
    var companies = await _commpany.GetQueryableAsync();

    var query = from inStock in inStocks
                join drug in drugs on inStock.DrugId equals drug.Id
                join company in companies on inStock.PharmaceuticalCompanyId equals company.Id
                select new DrugInStockCompanyFullDto
                {
                    // 药品入库
                    InStockId = inStock.Id,
                    DrugId = inStock.Id,
                    Quantity = inStock.Quantity,
                    UnitPrice = inStock.UnitPrice,
                    TotalAmount = inStock.TotalAmount,
                    ProductionDate = inStock.ProductionDate,
                    ExpiryDate = inStock.ExpiryDate,
                    BatchNumber = inStock.BatchNumber,
                    Supplier = inStock.Supplier,
                    Status = inStock.Status,
                    CreationTime = inStock.CreationTime,

                    // 药品管理
                    DrugName = drug.DrugName,
                    Specification = drug.Specification,
                    PurchasePrice = drug.PurchasePrice,
                    SalePrice = drug.SalePrice,
                    Stock = drug.Stock,
                    StockUpper = drug.StockUpper,
                    StockLower = drug.StockLower,
                    Effect = drug.Effect,
                    DrugProductionDate = drug.ProductionDate,
                    DrugExpiryDate = drug.ExpiryDate,


                    // 制药公司
                    CompanyId = company.Id,
                    CompanyName = company.CompanyName,
                    ContactPerson = company.ContactPerson,
                    ContactPhone = company.ContactPhone,
                    Address = company.Address,

                };

    var result = query.ToList();
    return ApiResult<List<DrugInStockCompanyFullDto>>.Success(result, ResultCode.Success);
}

    }
}
