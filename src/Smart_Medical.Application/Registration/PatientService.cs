using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Smart_Medical.Appointment;
using Smart_Medical.DoctorvVsit;
using Smart_Medical.Medical;
using Smart_Medical.OutpatientClinic.Dtos;
using Smart_Medical.OutpatientClinic.Dtos.Parameter;
using Smart_Medical.OutpatientClinic.IServices;
using Smart_Medical.Patient;
using Smart_Medical.Pharmacy;
using Smart_Medical.RBAC;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Smart_Medical.Registration
{
    /// <summary>
    /// 医疗管理
    /// </summary>
    [ApiExplorerSettings(GroupName = "医疗管理")]
    //[Authorize]
    [IgnoreAntiforgeryToken]
    public class PatientService : ApplicationService, IPatientService
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
        private readonly IRepository<Smart_Medical.Patient.Appointment, Guid> _appointment;
        private readonly IRepository<UserPatient, Guid> _userPatientRepo;

        public PatientService(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<DoctorClinic, Guid> doctorclinRepo,
            IRepository<BasicPatientInfo, Guid> basicpatientRepo,
            IRepository<Sick, Guid> sickRepo,
            IRepository<PatientPrescription, Guid> prescriptionRepo,
            IRepository<Drug, int> drugRepo,
            IRepository<Smart_Medical.Patient.Appointment, Guid> appointmentRep,
             IRepository<UserPatient, Guid> userPatientRepo
            )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _doctorclinRepo = doctorclinRepo;
            _patientRepo = basicpatientRepo;
            _sickRepo = sickRepo;
            _prescriptionRepo = prescriptionRepo;
            _drugRepo = drugRepo;
            _appointment = appointmentRep;
            _userPatientRepo = userPatientRepo;
        }

        /// <summary>
        /// 快速就诊、登记患者信息
        /// </summary>
        /// <param name="input">患者就诊登记信息</param>
        /// <returns>接口返回结果</returns>
        //[UnitOfWork]
        public async Task<ApiResult<ApiResult>> RegistrationPatientAsync(InsertPatientDto input)
        {
            try
            {
                using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
                {
                    try
                    {
                        // ================================
                        // 1. 创建或更新患者基本信息
                        // ================================

                        var existingPatient = await _patientRepo.FirstOrDefaultAsync(x => x.IdNumber == input.IdNumber);

                        BasicPatientInfo patient;

                        if (existingPatient == null)
                        {
                            // ---- 新建患者信息 ----
                            patient = ObjectMapper.Map<InsertPatientDto, BasicPatientInfo>(input);
                            patient.VisitStatus = "待就诊";
                            patient = await _patientRepo.InsertAsync(patient);
                        }
                        else
                        {
                            // ---- 更新已有患者信息 ----
                            patient = ObjectMapper.Map(input, existingPatient);
                            patient.VisitStatus = "待就诊";
                            patient = await _patientRepo.UpdateAsync(existingPatient);
                        }

                        if (patient == null)
                            return ApiResult<ApiResult>.Fail("患者信息登记失败，请稍后重试！", ResultCode.Error);

                        // ================================
                        // 2. 创建就诊流程记录
                        // ================================

                        var doctorClinic = ObjectMapper.Map<InsertPatientDto, DoctorClinic>(input);

                        doctorClinic.PatientId = patient.Id;
                        doctorClinic.VisitDateTime = patient.VisitDate;
                        doctorClinic.ExecutionStatus = ExecutionStatus.PendingConsultation;
                        doctorClinic.DispensingStatus = 0;
                        doctorClinic.VisitType = existingPatient == null ? "初诊" : "复诊";

                        if (await _doctorclinRepo.InsertAsync(doctorClinic) == null)
                            return ApiResult<ApiResult>.Fail("就诊流程创建失败，请稍后重试！", ResultCode.Error);

                        // ================================
                        // 3. 创建患者病历信息（初始化）
                        // ================================

                        var sick = new Sick
                        {
                            BasicPatientId = patient.Id,
                            Status = "新建",              // 病历状态必须有值，防止 Required 报错
                            InpatientNumber = "",

                            AdmissionDiagnosis = "",
                            DischargeTime = DateTime.Now,
                            CreationTime = patient.VisitDate,
                            Temperature = 36.5M,
                            Pulse = 75,
                            Breath = 18,
                            BloodPressure = "120/80"
                        };

                        if (await _sickRepo.InsertAsync(sick) == null)
                            return ApiResult<ApiResult>.Fail("病历信息创建失败，请稍后重试！", ResultCode.Error);
                        
                        // ================================
                        // 4. 提交事务，结束登记流程
                        // ================================

                        await uow.CompleteAsync();

                        return ApiResult<ApiResult>.Success(ApiResult.Success(ResultCode.Success), ResultCode.Success);
                    }
                    catch (Exception ex)
                    {
                        // ================================
                        // 异常处理（输出异常信息）
                        // ================================
                        return ApiResult<ApiResult>.Fail("系统异常：" + ex.Message, ResultCode.Error);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        /// <summary>
        /// 就诊患者
        /// </summary>
        /// <param name="input">参数列表，包含分页和关键词</param>
        /// <returns></returns>
        public async Task<ApiResult<PagedResultDto<GetVisitingDto>>> VisitingPatientsAsync(GetVistingParameterDtos input)
        {
            try
            {
                // 1. 转换 VisitStatus 参数为字符串
                string status = input.VisitStatus switch
                {
                    1 => "待就诊",
                    2 => "已就诊",
                    //  _   "弃元"  :  "匹配所有不符合前面条件的值"
                    _ => throw new Exception("就诊信息异常")
                };

                // 2. 获取 IQueryable 查询对象（还没执行数据库）
                var query = await _patientRepo.GetQueryableAsync();

                // 3. 根据 VisitStatus 筛选患者
                var filteredQuery = query.Where(x => x.VisitStatus == status);

                // 4. 关键词模糊匹配身份证号、姓名、电话
                if (!string.IsNullOrWhiteSpace(input.Keyword))
                {
                    string keyword = input.Keyword.Trim();
                    filteredQuery = filteredQuery.Where(x =>
                        x.IdNumber.Contains(keyword) ||
                        x.PatientName.Contains(keyword) ||
                        x.ContactPhone.Contains(keyword)
                    );
                }

                // 5. 计算总条数（分页用）
                var totalCount = await AsyncExecuter.CountAsync(filteredQuery);

                // 6. 分页查询，跳过前面页数的数据，取当前页数据
                var pagedPatients = await AsyncExecuter.ToListAsync(
                    filteredQuery
                        .Page(input.PageIndex, input.PageSize)
                );

                // 7. 实体转 DTO
                var result = ObjectMapper.Map<List<BasicPatientInfo>, List<GetVisitingDto>>(pagedPatients);
                // 8. 返回分页结果
                return ApiResult<PagedResultDto<GetVisitingDto>>.Success(
                    new PagedResultDto<GetVisitingDto>(totalCount, result),
                    ResultCode.Success
                );
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 就诊患者详细信息
        /// </summary>
        /// <param name="patientId">患者id</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResult<BasicPatientInfoDto>> GetPatientInfoAsync(Guid patientId)
        {
            try
            {
                var patientInfo = await _patientRepo.FindAsync(patientId);
                if (patientInfo == null)
                {
                    return ApiResult<BasicPatientInfoDto>.Fail("未找到该患者信息", ResultCode.NotFound);
                }
                var result = ObjectMapper.Map<BasicPatientInfo, BasicPatientInfoDto>(patientInfo);
                return ApiResult<BasicPatientInfoDto>.Success(result, ResultCode.Success);
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// 患者所有病历信息
        /// </summary>
        /// <param name="patientId">患者id</param>
        /// <returns></returns>        
        public async Task<ApiResult<List<GetSickInfoDto>>> GetPatientSickInfoAsync(Guid patientId)
        {
            try
            {
                // 获取所有患者、就诊记录、病历记录、处方记录的 IQueryable 数据源
                var patients = await _patientRepo.GetQueryableAsync();
                var clinics = await _doctorclinRepo.GetQueryableAsync();
                var sicks = await _sickRepo.GetQueryableAsync();
                var prescriptions = await _prescriptionRepo.GetQueryableAsync();

                // 执行联表查询：基于 patientId 联合就诊记录、病历记录、处方记录
                var query = from p in patients
                            where p.Id == patientId
                            join c in clinics on p.Id equals c.PatientId
                            where c.ExecutionStatus == ExecutionStatus.Completed ||
                                  c.ExecutionStatus == ExecutionStatus.Cancelled ||
                                  c.ExecutionStatus == ExecutionStatus.PendingEvaluation
                            // Left join 病历
                            join s in sicks on p.Id equals s.BasicPatientId into sickGroup
                            from s in sickGroup.DefaultIfEmpty()
                                // Left join 处方
                            join pr in prescriptions on p.Id equals pr.PatientNumber into presGroup
                            from pr in presGroup.DefaultIfEmpty()

                                // 构建返回的 DTO
                            select new GetSickInfoDto
                            {
                                BasicPatientId = pr.Id,
                                Temperature = s.Temperature, // 体温
                                Pulse = s.Pulse,             // 脉搏
                                Breath = s.Breath,           // 呼吸
                                BloodPressure = s.BloodPressure, // 血压
                                ChiefComplaint = c.ChiefComplaint, // 主诉
                                DrugIds = pr.DrugIds,                   // 药品 ID 字符串
                                PrescriptionTemplateNumber = pr.PrescriptionTemplateNumber, // 模板编号
                                MedicalAdvice = pr.MedicalAdvice ?? ""      // 医嘱
                            };

                var result = query
                            .AsEnumerable()
                            .GroupBy(item => new
                            {
                                item.BasicPatientId,
                            })
                            .Select(g => g.First()) // 每组只保留第一个
                            .Select(item => new GetSickInfoDto
                            {
                                BasicPatientId = item.BasicPatientId,
                                Temperature = item.Temperature,
                                Pulse = item.Pulse,
                                Breath = item.Breath,
                                BloodPressure = item.BloodPressure,
                                ChiefComplaint = item.ChiefComplaint,
                                PrescriptionTemplateNumber = item.PrescriptionTemplateNumber,
                                MedicalAdvice = item.MedicalAdvice,
                                DrugItems = JsonConvert.DeserializeObject<List<DrugItemDto>>(item.DrugIds ?? "") ?? new List<DrugItemDto>(),
                            })
                            .ToList();
                foreach (var item in result)
                {
                    foreach (var drug in item.DrugItems)
                    {
                        var drugInfo = await _drugRepo.GetAsync(drug.DrugId);
                        if (drugInfo != null)
                        {
                            drug.DrugName = drugInfo.DrugName;
                        }
                    }
                }

                // 返回成功结果
                return ApiResult<List<GetSickInfoDto>>.Success(result, ResultCode.Success);
            }
            catch (Exception ex)
            {
                return ApiResult<List<GetSickInfoDto>>.Fail("系统异常：" + ex.Message, ResultCode.Error);
            }
        }

        /// <summary>
        /// 开具处方
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>        
        public async Task<ApiResult<ApiResult>> DoctorsPrescription(DoctorPrescriptionDto input)
        {
            try
            {
                using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
                {
                    //判断输入参数是否完整
                    if (input == null || input.PatientNumber == Guid.Empty)
                        return ApiResult<ApiResult>.Fail("患者信息不完整！", ResultCode.Error);

                    //如果使用处方模板，则必须提供模板编号
                    if (input.IsActive)
                    {
                        if (input.PrescriptionItems == null)
                        {
                            return ApiResult<ApiResult>.Fail("处方项不能为空！", ResultCode.Error);
                        }
                    }

                    // 获取患者基本信息
                    var patient = await _patientRepo.FindAsync(input.PatientNumber);
                    if (patient == null)
                        return ApiResult<ApiResult>.Fail("未找到该患者信息！", ResultCode.NotFound);

                    // =============================================
                    // 根据 PrescriptionItems 插入处方明细表
                    // =============================================

                    //判断每种药品是否存在、库存是否充足
                    foreach (var item in input.PrescriptionItems)
                    {
                        // 获取药品信息
                        var drug = await _drugRepo.FirstOrDefaultAsync(d => d.Id == item.DrugId);
                        //药品不存在返回错误信息
                        if (drug == null)
                            return ApiResult<ApiResult>.Fail($"未找到药品ID为 {item.DrugId} 的药品信息", ResultCode.Error);

                        //查找的药品库存是否充足
                        int remainingStock = drug.Stock - item.Number;
                        if (remainingStock < 0)
                            return ApiResult<ApiResult>.Fail($"药品 {drug.DrugName} 库存不足，无法开具处方", ResultCode.Error);
                        
                        //提前 return，但没有调用 uow.CompleteAsync()，那事务是不会提交的，会自动回滚
                        // 更新药品库存
                        drug.Stock = remainingStock;
                        //await _drugRepo.UpdateAsync(drug);
                    }

                    //创建处方记录
                    var prescription = new PatientPrescription
                    {
                        PrescriptionTemplateNumber = input.PrescriptionTemplateNumber,
                        PatientNumber = input.PatientNumber,
                        IsActive = input.IsActive,
                        // 序列化处方明细
                        DrugIds = JsonConvert.SerializeObject(input.PrescriptionItems),
                        MedicalAdvice = input.MedicalAdvice
                    };
                    //保存处方记录
                    await _prescriptionRepo.InsertAsync(prescription);

                    //更新患者状态为"已就诊"
                    patient.VisitStatus = "已就诊";
                    await _patientRepo.UpdateAsync(patient);//保留

                    //更新 DoctorClinic 表的状态字段 
                    //判断状态为"待就诊"
                    var doctorClinic = await _doctorclinRepo.FirstOrDefaultAsync(
                        x => x.PatientId == input.PatientNumber &&
                        x.ExecutionStatus == ExecutionStatus.PendingConsultation
                        );

                    if (doctorClinic == null)
                        return ApiResult<ApiResult>.Fail("未找到就诊记录！", ResultCode.NotFound);

                    // 更新就诊记录状态为"已就诊"
                    doctorClinic.ExecutionStatus = ExecutionStatus.Completed;
                    await _doctorclinRepo.UpdateAsync(doctorClinic);

                    await uow.CompleteAsync();

                    return ApiResult<ApiResult>.Success(ApiResult.Success(ResultCode.Success), ResultCode.Success);
                }
            }
            catch (Exception ex)
            {
                return ApiResult<ApiResult>.Fail("系统错误：" + ex.Message, ResultCode.Error);
            }
        }

        /// <summary>
        /// 线上预约
        /// </summary>
        /// <returns></returns>
        [UnitOfWork]// 添加 [UnitOfWork] 特性，确保此方法的数据库操作在事务中执行
        [HttpPost]
        public async Task<ApiResult> InsertMakeAppointment(MakeAppointmentDto make)
        {
            if (make == null)
            {
                return ApiResult.Fail("预约信息不能为空！", ResultCode.Error);
            }

            //检查患者信息是否存在
            var res = await _patientRepo.FirstOrDefaultAsync(x => x.IdNumber == make.IdNumber);
            //查询数据库是否存在此患者  没有此患者信息则添加
            if (res == null)
            {
                //1 先添加患者信息 
                var basicPatientInfo = ObjectMapper.Map<MakeAppointmentDto, BasicPatientInfo>(make);

                //检查患者信息是否完整
                if (string.IsNullOrWhiteSpace(basicPatientInfo.PatientName) ||
                    string.IsNullOrWhiteSpace(basicPatientInfo.IdNumber) ||
                    string.IsNullOrWhiteSpace(basicPatientInfo.ContactPhone))
                {
                    return ApiResult.Fail("患者信息不完整，请检查后重试！", ResultCode.Error);
                }

                //添加患者信息
                var patient = await _patientRepo.InsertAsync(basicPatientInfo);

                //拿到刚刚添加的患者信息的id
                make.PatientId = patient?.Id;
            }
            // 如果患者信息已经存在，则直接使用现有的 PatientId
            else
            {
                make.PatientId = res.Id;
            }
            //2 添加预约挂号记录
            Smart_Medical.Patient.Appointment appointment = new Smart_Medical.Patient.Appointment
            {
                PatientId = make.PatientId,
                AppointmentDateTime = make.AppointmentDateTime,
                Status = make.Status,
                ActualFee = make.ActualFee,
                Remarks = make.Remarks
            };
            var appointmentResult = await _appointment.InsertAsync(appointment);

            return ApiResult.Success(ResultCode.Success);
        }


        /// <summary>
        /// 获取预约挂号分页列表
        /// </summary>
        /// <param name="input">查询参数（包含就诊状态、就诊日期、分页信息）</param>
        /// <returns>ApiResult 分页结果</returns>
        public async Task<ApiResult<PageResult<List<AppointmentReponseDto>>>> GetAppointment([FromQuery] AppointmentDto input)
        {
            try
            {
                // 1. 获取预约挂号表的 IQueryable
                var queryable = await _appointment.GetQueryableAsync();
                queryable = queryable.Where(x => x.PatientId == input.PatientId); // 只查询当前患者的预约信息
                // 2. 联查患者基本信息（外键 PatientId -> BasicPatientInfo.Id）
                var patientQueryable = await _patientRepo.GetQueryableAsync();

                var query = from a in queryable
                            join p in patientQueryable on a.PatientId equals p.Id
                            select new { Appointment = a, Patient = p };

                // 3. 条件过滤
                if (!string.IsNullOrWhiteSpace(input.VisitStatus))
                {
                    query = query.Where(x => x.Appointment.Status.ToString() == input.VisitStatus);
                }
                if (input.VisitDate != null)
                {
                    var date = input.VisitDate;
                    query = query.Where(x => x.Appointment.AppointmentDateTime.Date == date);
                }

                // 4. 获取总数
                var totalCount = await AsyncExecuter.CountAsync(query);

                // 5. 分页
                var pageData = await AsyncExecuter.ToListAsync(
                    query
                        .OrderByDescending(x => x.Appointment.AppointmentDateTime)
                        .Skip(input.SkipCount)
                        .Take(input.MaxResultCount)
                );

                // 6. 映射到响应 DTO
                var resultList = pageData.Select(x => new AppointmentReponseDto
                {
                    PatientId = x.Patient.Id,
                    AppointmentDateTime = x.Appointment.AppointmentDateTime,
                    Status = x.Appointment.Status,
                    ActualFee = x.Appointment.ActualFee,
                    Remarks = x.Appointment.Remarks,
                    VisitId = x.Patient.VisitId,
                    PatientName = x.Patient.PatientName,
                    Gender = x.Patient.Gender,
                    Age = x.Patient.Age,
                    AgeUnit = x.Patient.AgeUnit,
                    ContactPhone = x.Patient.ContactPhone,
                    IdNumber = x.Patient.IdNumber,
                    VisitType = x.Patient.VisitType,
                    IsInfectiousDisease = x.Patient.IsInfectiousDisease,
                    DiseaseOnsetTime = x.Patient.DiseaseOnsetTime,
                    EmergencyTime = x.Patient.EmergencyTime,
                    VisitStatus = x.Appointment.Status.ToString(),
                    VisitDate = x.Appointment.AppointmentDateTime.Date
                }).ToList();

                // 7. 构造分页结果
                var pageResult = new PageResult<List<AppointmentReponseDto>>
                {
                    TotleCount = totalCount,
                    TotlePage = (int)Math.Ceiling((double)totalCount / input.MaxResultCount),
                    Data = resultList
                };

                return ApiResult<PageResult<List<AppointmentReponseDto>>>.Success(pageResult, ResultCode.Success);
            }
            catch (Exception ex)
            {
                return ApiResult<PageResult<List<AppointmentReponseDto>>>.Fail("获取预约列表失败：" + ex.Message, ResultCode.Error);
            }
        }


        /// <summary>
        /// 查询当前用户下所有关联患者
        /// </summary>
        public async Task<ApiResult<List<UserPatientDto>>> GetMyPatientsAsync(Guid? userId)
        {
            Guid queryUserId;

            // 优先使用传入的 userId 参数，如果为空则使用 CurrentUser.Id
            if (userId.HasValue)
            {
                queryUserId = userId.Value;
            }
            else
            {
                var userIdStr = CurrentUser?.Id?.ToString();
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out queryUserId))
                {
                    return ApiResult<List<UserPatientDto>>.Fail("未获取到当前用户信息", ResultCode.Error);
                }
            }

            // 使用 WithDetailsAsync 联查 UserPatient 及其关联的 BasicPatientInfo 实体
            var userPatients = await _userPatientRepo.WithDetailsAsync(up => up.Patient);

            var query = userPatients
                        .Where(up => up.UserId == queryUserId)
                        .Select(up => new UserPatientDto
                        {
                            PatientId = up.Patient.Id,
                            VisitId = up.Patient.VisitId,
                            PatientName = up.Patient.PatientName,
                            Gender = up.Patient.Gender,
                            Age = up.Patient.Age,
                            AgeUnit = up.Patient.AgeUnit,
                            ContactPhone = up.Patient.ContactPhone,
                            IdNumber = up.Patient.IdNumber,
                            VisitType = up.Patient.VisitType,
                            IsInfectiousDisease = up.Patient.IsInfectiousDisease,
                            DiseaseOnsetTime = up.Patient.DiseaseOnsetTime,
                            EmergencyTime = up.Patient.EmergencyTime,
                            VisitStatus = up.Patient.VisitStatus,
                            VisitDate = up.Patient.VisitDate
                        });

            var result = await AsyncExecuter.ToListAsync(query);
            return ApiResult<List<UserPatientDto>>.Success(result, ResultCode.Success);
        }

    }
}


