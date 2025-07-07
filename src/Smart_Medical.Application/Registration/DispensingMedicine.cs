using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Smart_Medical.DoctorvVsit;
using Smart_Medical.Medical;
using Smart_Medical.OutpatientClinic.Dtos;
using Smart_Medical.OutpatientClinic.Dtos.Parameter;
using Smart_Medical.Patient;
using Smart_Medical.Pharmacy;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Smart_Medical.Registration
{
    /// <summary>
    /// 收费发药
    /// </summary>
    [ApiExplorerSettings(GroupName = "收费发药管理")]
    public class DispensingMedicine : ApplicationService
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

        private readonly ILogger<DispensingMedicine> _logger;

        /// <summary>
        /// 构造函数注入
        /// </summary>
        public DispensingMedicine(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<DoctorClinic, Guid> doctorclinRepo,
            IRepository<BasicPatientInfo, Guid> basicpatientRepo,
            IRepository<Sick, Guid> sickRepo,
            IRepository<PatientPrescription, Guid> prescriptionRepo,
            IRepository<Drug, int> drugRepo,
            ILogger<DispensingMedicine> logger) // 注入日志服务
        {
            _unitOfWorkManager = unitOfWorkManager;
            _doctorclinRepo = doctorclinRepo;
            _patientRepo = basicpatientRepo;
            _sickRepo = sickRepo;
            _prescriptionRepo = prescriptionRepo;
            _drugRepo = drugRepo;
            _logger = logger;
        }

        /// <summary>
        /// 发药接口，根据患者编号统一处理患者的所有处方药品发药逻辑
        /// </summary>
        /// <param name="patientNumber">患者编号（GUID）</param>
        /// <returns>返回 ApiResult，标识发药成功或失败及相关提示信息</returns>
        public async Task<ApiResult> DistributeMedicine(Guid patientNumber)
        {
            // 身份证校验，患者编号不能是空Guid
            if (patientNumber == Guid.Empty)
            {
                return ApiResult.Fail("患者编号不能为空！你这是想给空气发药吗？😂", ResultCode.Error); // 修正为BadRequest更合适
            }

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true)) // 开启一个新的事务单元
            {
                try
                {
                    //确认患者存在 患者都不在，发什么药？
                    var patient = await _patientRepo.FirstOrDefaultAsync(p => p.Id == patientNumber);
                    if (patient == null)
                    {
                        _logger.LogWarning($"发药失败：找不到患者编号为 {patientNumber} 的患者。");
                        return ApiResult.Fail("患者信息不存在！发药对象找不着了呢！😭", ResultCode.NotFound);
                    }

                    var doctorClinic = await _doctorclinRepo.FirstOrDefaultAsync(dc => dc.PatientId == patientNumber && dc.DispensingStatus == 0); // 找到待发药的就诊流程
                    if (doctorClinic == null)
                    {
                        _logger.LogInformation($"患者 {patientNumber} 没有需要发药的就诊流程。");
                        return ApiResult.Fail("患者没有待发药的就诊记录！别瞎忙活啦！😊", ResultCode.Success); // 没有待发药的也算成功吧，毕竟不用发了
                    }

                    var prescriptions = await _prescriptionRepo.GetQueryableAsync();
                    prescriptions = prescriptions.Where(p => p.PatientNumber == patientNumber);

                    if (!prescriptions.Any())
                    {
                        _logger.LogInformation($"患者 {patientNumber} 没有开具任何处方。");
                        // 即使没有处方，但就诊流程是待发药，也应该处理成“已发药”状态，表示没有需要发售的药品
                        doctorClinic.DispensingStatus = 1; // 已发药
                        await _doctorclinRepo.UpdateAsync(doctorClinic);
                        await uow.CompleteAsync(); // 提交事务
                        return ApiResult.Success(ResultCode.Success);
                    }

                    //遍历所有处方，处理药品发药逻辑
                    foreach (var prescription in prescriptions)
                    {
                        if (prescription.IsActive) // 如果是使用处方模板，DrugIds是逗号分隔的字符串
                        {
                            if (!string.IsNullOrWhiteSpace(prescription.DrugIds))
                            {
                                var drugIds = prescription.DrugIds.Split(',')
                                                        .Select(int.Parse)
                                                        .ToList();

                                foreach (var drugId in drugIds)
                                {
                                    var drug = await _drugRepo.FirstOrDefaultAsync(d => d.Id == drugId);
                                    if (drug == null)
                                    {
                                        _logger.LogError($"发药失败：处方 {prescription.Id} 中的药品ID {drugId} 不存在。");
                                        throw new UserFriendlyException($"处方中包含无效药品ID: {drugId}。发药失败！");
                                    }
                                    // 为了简化，这里暂时不处理具体的库存扣减，但这是真实场景下必须的！
                                    _logger.LogInformation($"已处理处方 {prescription.Id} 中的药品 {drug.Id}。");
                                }
                            }
                        }
                        else // 如果是不使用处方模板
                        {
                            if (!string.IsNullOrWhiteSpace(prescription.DrugIds))
                            {
                                try
                                {
                                    // 解析JSON字符串到List<PrescriptionItemDto>
                                    // 注意：这里需要引入 NewtonSoft.Json 或 System.Text.Json
                                    var prescriptionItems = JsonConvert.DeserializeObject<List<PrescriptionItemDto>>(prescription.DrugIds);

                                    if (prescriptionItems != null && prescriptionItems.Any())
                                    {
                                        foreach (var item in prescriptionItems)
                                        {
                                            var drug = await _drugRepo.FirstOrDefaultAsync(d => d.Id == item.DrugId);
                                            if (drug == null)
                                            {
                                                _logger.LogError($"发药失败：处方 {prescription.Id} 手动录入的药品ID {item.DrugId} 不存在。");
                                                throw new UserFriendlyException($"手动录入处方中包含无效药品ID: {item.DrugId}。发药失败！");
                                            }
                                            ;
                                            _logger.LogInformation($"已处理处方 {prescription.Id} 手动录入的药品 {drug.Id}，数量 {item.Number}。");
                                        }
                                    }
                                }
                                catch (JsonException jsonEx)
                                {
                                    _logger.LogError(jsonEx, $"解析处方 {prescription.Id} 的药品明细JSON失败：{prescription.DrugIds}");
                                    throw new UserFriendlyException("处方药品数据格式错误，请联系管理员！💊");
                                }
                            }
                        }
                        // 这里可以增加对单个处方的“已发药”标记，如果需要的话
                    }

                    // 4. 更新就诊流程表的状态
                    doctorClinic.DispensingStatus = 1; // 设置为“已发药”
                    doctorClinic.ExecutionStatus = ExecutionStatus.Completed; // 可以同时更新就诊状态为“已就诊”或“待评价”
                    await _doctorclinRepo.UpdateAsync(doctorClinic);

                    // 提交事务
                    await uow.CompleteAsync();
                    return ApiResult.Success(ResultCode.Success);
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync(); // 发生异常，先别慌，回滚事务，别把数据搞乱了！
                    _logger.LogError(ex, $"发药失败，系统异常！患者编号：{patientNumber}");
                    return ApiResult.Fail("发药失败，系统异常！程序它有点小情绪了呢！😢", ResultCode.Error); // 返回系统异常提示
                }
            }
        }

        /// <summary>
        /// 本次看诊
        /// </summary>
        /// <param name="IdNumber">身份证号</param>
        /// <returns></returns>
        public async Task<ApiResult<List<GetSickInfoDto>>> ConsultationRecord(string IdNumber)
        {
            try
            {
                //过于极端了
                //var existingPatient = (await _patientRepo
                //    .FirstOrDefaultAsync(
                //        x => x.IdNumber == IdNumber) ?? throw new Exception("患者身份信息有误")
                //    ).Id;
                var existingPatient = await _patientRepo
                    .FirstOrDefaultAsync(x => x.IdNumber == IdNumber);

                if (existingPatient == null)
                {
                    return ApiResult<List<GetSickInfoDto>>.Fail("身份证号不能为空", ResultCode.NotFound);
                }
                // 获取所有患者、就诊记录、病历记录、处方记录的 IQueryable 数据源
                var patients = await _patientRepo.GetQueryableAsync();
                var clinics = await _doctorclinRepo.GetQueryableAsync();
                var sicks = await _sickRepo.GetQueryableAsync();

                // 执行联表查询：基于 patientId 联合就诊记录、病历记录、处方记录
                var query = from p in patients
                            where p.IdNumber == IdNumber
                            join c in clinics on p.Id equals c.PatientId
                            where c.ExecutionStatus == ExecutionStatus.PendingConsultation                                  
                            join s in sicks on p.Id equals s.BasicPatientId into sickGroup
                            from s in sickGroup.DefaultIfEmpty()
                            select new GetSickInfoDto
                            {
                                Temperature = s.Temperature, // 体温
                                Pulse = s.Pulse,             // 脉搏
                                Breath = s.Breath,           // 呼吸
                                BloodPressure = s.BloodPressure, // 血压
                                ChiefComplaint = c.ChiefComplaint, // 主诉
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
                            })
                            .ToList();                
                return ApiResult<List<GetSickInfoDto>>.Success(result, ResultCode.Success);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

}

