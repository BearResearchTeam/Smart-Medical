using Microsoft.AspNetCore.Mvc;
using Smart_Medical.DoctorvVsit;
using Smart_Medical.Medical;
using Smart_Medical.Patient;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Validation.StringValues;

namespace Smart_Medical.PatientManagement
{

    [ApiExplorerSettings(GroupName = "患者管理")]
    public class PatientService : ApplicationService, IPatientService
    {
        /// <summary>
        /// 就诊流程
        /// </summary>
        private readonly IRepository<DoctorClinic, Guid> _doctorclinRepo;
        /// <summary>
        /// 患者病历信息
        /// </summary>
        private readonly IRepository<Sick, Guid> _sickRepo;
        /// <summary>
        /// 患者开具处方
        /// </summary>
        private readonly IRepository<PatientPrescription, Guid> _prescriptionRepo;
        /// <summary>
        /// 患者信息
        /// </summary>
        private readonly IRepository<BasicPatientInfo, Guid> _patientRepository;


        private readonly IRepository<Smart_Medical.Patient.Appointment, Guid> _repository;

        public PatientService(
                IRepository<DoctorClinic, Guid> doctorclinRepo,
                IRepository<Sick, Guid> sickRepo,
                IRepository<PatientPrescription, Guid> prescriptionRepo,
                IRepository<BasicPatientInfo, Guid> patientRepository,            
                IRepository<Patient.Appointment, Guid> repository

            )
        {
            _doctorclinRepo = doctorclinRepo;
            _sickRepo = sickRepo;
            _prescriptionRepo = prescriptionRepo;
            _patientRepository = patientRepository;
            _repository = repository;
            
        }

        [UnitOfWork]
        public async Task<ApiResult> DeletePatientInfo(Guid PatId)
        {
            try
            {
                var patient = await _patientRepository.GetAsync(PatId);
                if (patient == null)
                    return ApiResult.Fail("未找到要删除的患者信息", ResultCode.NotFound);

                var doctorclin = await _doctorclinRepo.GetQueryableAsync();
                doctorclin = doctorclin.Where(x => x.PatientId == PatId);
                if (doctorclin != null)
                    await _doctorclinRepo.DeleteManyAsync(doctorclin);

                var sick = await _sickRepo.GetQueryableAsync();
                sick = sick.Where(x => x.BasicPatientId == PatId);
                if (sick != null)
                    await _sickRepo.DeleteManyAsync(sick);

                var prescription = await _prescriptionRepo.GetQueryableAsync();
                prescription = prescription.Where(x => x.PatientNumber == PatId);
                if (prescription != null)
                    await _prescriptionRepo.DeleteManyAsync(prescription);


                await _patientRepository.DeleteAsync(PatId);
                return ApiResult.Success(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"删除患者信息失败: {ex.Message}", ResultCode.Error);
            }
        }
        /// <summary>
        /// 查询所有患者信息（分页+模糊搜索）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ApiResult<PageResultDto<GetInsertPatientDto>>> GetPatientInfoAsync(GetVistingParameterDtos input)
        {
            try
            {
                var cases = _repository.GetQueryableAsync();
                var queryable = await _patientRepository.GetQueryableAsync();

                if (!string.IsNullOrWhiteSpace(input.Keyword))
                {
                    input.Keyword = input.Keyword.Trim();
                    queryable = queryable.Where(x =>
                        x.PatientName.Contains(input.Keyword) ||
                        x.ContactPhone.Contains(input.Keyword) ||
                        x.IdNumber.Contains(input.Keyword)
                    );
                }

                var totalCount = await AsyncExecuter.CountAsync(queryable);

                var data = await AsyncExecuter.ToListAsync(
                    queryable.OrderByDescending(x => x.CreationTime)
                             .Skip((input.PageIndex - 1) * input.PageSize)
                             .Take(input.PageSize)
                );

                var result = new PageResultDto<GetInsertPatientDto>(
                        totalCount,
                        ObjectMapper.Map<List<BasicPatientInfo>, List<GetInsertPatientDto>>(data)
                    );
                return ApiResult<PageResultDto<GetInsertPatientDto>>.Success(
                       result, ResultCode.Success
                    );

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<ApiResult> UpdatePatientInfo(UpdPatientDto updPatientDto)
        {
            try
            {
                var patient = await _patientRepository.GetAsync(updPatientDto.ID);
                if (patient == null)
                {
                    return ApiResult.Fail("未找到要更新的患者信息", ResultCode.NotFound);
                }

                ObjectMapper.Map(updPatientDto, patient);

                await _patientRepository.UpdateAsync(patient);
                return ApiResult.Success(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"更新患者信息失败: {ex.Message}", ResultCode.Error);
            }
        }

    }
}