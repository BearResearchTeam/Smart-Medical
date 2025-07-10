using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
using Volo.Abp.ObjectMapping;
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
        public async Task<ApiResult<ApiResult>> DistributeMedicine(Guid patientNumber)
        {
            // 身份证校验，患者编号不能是空Guid
            if (patientNumber == Guid.Empty)
            {
                return ApiResult<ApiResult>.Fail("患者编号不能为空！你这是想给空气发药吗？😂", ResultCode.Error); // 修正为BadRequest更合适
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
                        return ApiResult<ApiResult>.Fail("患者信息不存在！发药对象找不着了呢！😭", ResultCode.NotFound);
                    }

                    var doctorClinic = await _doctorclinRepo.FirstOrDefaultAsync(dc => dc.PatientId == patientNumber && dc.DispensingStatus == 0); // 找到待发药的就诊流程
                    if (doctorClinic == null)
                    {
                        _logger.LogInformation($"患者 {patientNumber} 没有需要发药的就诊流程。");
                        return ApiResult<ApiResult>.Fail("患者没有待发药的就诊记录！别瞎忙活啦！😊", ResultCode.Success); // 没有待发药的也算成功吧，毕竟不用发了
                    }

                    var prescriptions = await _prescriptionRepo.GetQueryableAsync();
                    prescriptions = prescriptions.Where(p => p.PatientNumber == patientNumber);
                    prescriptions = prescriptions.OrderByDescending(x => x.CreationTime);
                    var latestPrescription = prescriptions.FirstOrDefault();

                    if (!prescriptions.Any())
                    {
                        _logger.LogInformation($"患者 {patientNumber} 没有开具任何处方。");
                        // 即使没有处方，但就诊流程是待发药，也应该处理成“已发药”状态，表示没有需要发售的药品
                        doctorClinic.DispensingStatus = 1; // 已发药
                        await _doctorclinRepo.UpdateAsync(doctorClinic);
                        await uow.CompleteAsync(); // 提交事务
                        return ApiResult<ApiResult>.Success(
                            ApiResult.Success(ResultCode.Success),ResultCode.Success
                            );
                    }

                    //遍历所有处方，处理药品发药逻辑
                    foreach (var prescription in prescriptions)
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
                                                return ApiResult<ApiResult>.Fail($"手动录入处方中包含无效药品ID: {item.DrugId}。发药失败！", ResultCode.NotFound);

                                            //查找的药品库存是否充足
                                            int remainingStock = drug.Stock - item.Number;
                                            if (remainingStock < 0)
                                                return ApiResult<ApiResult>.Fail($"药品 {drug.DrugName} 库存不足，无法开具发药", ResultCode.Error);

                                            // 更新药品库存
                                            drug.Stock = remainingStock;
                                            await _drugRepo.UpdateAsync(drug);
                                        }
                                    }
                                }
                                catch (JsonException jsonEx)
                                {
                                    _logger.LogError(jsonEx, $"解析处方 {prescription.Id} 的药品明细JSON失败：{prescription.DrugIds}");
                                    throw new UserFriendlyException("处方药品数据格式错误，请联系管理员！💊");
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
                    return ApiResult<ApiResult>.Success(
                            ApiResult.Success(ResultCode.Success), ResultCode.Success
                            );
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync(); // 发生异常，先别慌，回滚事务，别把数据搞乱了！
                    _logger.LogError(ex, $"发药失败，系统异常！患者编号：{patientNumber}");
                    return ApiResult<ApiResult>.Fail("发药失败，系统异常！程序它有点小情绪了呢！😢", ResultCode.Error); // 返回系统异常提示
                }
            }
        }

        /// <summary>
        /// 本次看诊
        /// </summary>
        /// <param name="IdNumber">身份证号</param>
        /// <returns></returns>
        public async Task<ApiResult<List<DrugItemDto>>> ConsultationRecord(Guid Id)
        {
            try
            {
                var patientPresc = await _prescriptionRepo.GetQueryableAsync();
                patientPresc = patientPresc.Where(x => x.PatientNumber == Id);
                patientPresc = patientPresc.OrderByDescending(x => x.CreationTime);
                var latestPrescription = patientPresc.FirstOrDefault();
                var result = JsonConvert.DeserializeObject<List<DrugItemDto>>(latestPrescription.DrugIds ?? "") ?? new List<DrugItemDto>();
                foreach (var drug in result)
                {
                    var drugInfo = await _drugRepo.GetAsync(drug.DrugId);
                    if (drugInfo != null)
                    {
                        drug.DrugName = drugInfo.DrugName;
                    }
                }


                return ApiResult<List<DrugItemDto>>.Success(result, ResultCode.Success);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ApiResult<PagedResultDto<GetVisitingDto>>> VisitingPatientsAsync(GetVistingParameterDto input)
        {
            try
            {
                var query = from p in await _patientRepo.GetQueryableAsync()
                            join c in await _doctorclinRepo.GetQueryableAsync() on p.Id equals c.PatientId
                            where p.VisitStatus == "已就诊"
                                  && c.DispensingStatus == input.DispensingStatus
                                  && c.VisitDateTime.Date == DateTime.Today.Date // 只查今天
                            select new
                            {
                                Patient = p,
                                Clinic = c
                            };

                //关键词模糊搜索
                if (!string.IsNullOrWhiteSpace(input.Keyword))
                {
                    var keyword = input.Keyword.Trim();
                    query = query.Where(x =>
                        x.Patient.IdNumber.Contains(keyword) ||
                        x.Patient.PatientName.Contains(keyword) ||
                        x.Patient.ContactPhone.Contains(keyword)
                    );
                }

                var totalCount = await AsyncExecuter.CountAsync(query);

                var result = await AsyncExecuter.ToListAsync(
                    query.Page(input.PageIndex, input.PageSize)
                         .Select(x => new GetVisitingDto
                         {
                             Id = x.Patient.Id,
                             PatientName = x.Patient.PatientName,
                             Gender = x.Patient.Gender,
                             Age = x.Patient.Age,
                             VisitDate = x.Patient.VisitDate,
                         })
                );

                // 6. 返回分页结果
                return ApiResult<PagedResultDto<GetVisitingDto>>.Success(
                    new PagedResultDto<GetVisitingDto>(totalCount, result),
                    ResultCode.Success
                );
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw;
            }
        }

    }

}

