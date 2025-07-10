using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using Smart_Medical.DoctorvVsit;
using Smart_Medical.Medical;
using Smart_Medical.OutpatientClinic.Dtos;
using Smart_Medical.OutpatientClinic.Dtos.Parameter;
using Smart_Medical.OutpatientClinic.IServices;
using Smart_Medical.Patient;
using Smart_Medical.Registration.Dtos;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Registration
{
    



    /// <summary>
    /// 住院管理
    /// </summary>
    [ApiExplorerSettings(GroupName = "住院管理")]
    public class HospitalizedService : ApplicationService, IHospitalizedService
    {
        private readonly IRepository<Sick, Guid> _sickRepository;
        private readonly IRepository<BasicPatientInfo, Guid> _patientRepository;
        private readonly IRepository<DoctorClinic, Guid> _clinicRepository;
        private readonly IRepository<DoctorAccount, Guid> _doctorRepository;


        public HospitalizedService(
            IRepository<DoctorAccount,Guid> doctorRepository,
            IRepository<DoctorClinic, Guid> clinicRepository,
            IRepository<Sick, Guid> sickRepository, 
            IRepository<BasicPatientInfo, Guid> patientRepository
            )
        {
            _sickRepository = sickRepository;
            _patientRepository = patientRepository;
            _clinicRepository = clinicRepository;
            _doctorRepository = doctorRepository;
        }

        #region

        /// <summary>
        /// 办理入院
        /// </summary>
        public async Task<ApiResult<ApiResult>> CreateAsync(Guid id)
        {
            try
            {
                var queryable = await _sickRepository.GetQueryableAsync();
                var sick = await queryable
                    .FirstOrDefaultAsync(x => x.BasicPatientId == id && x.Status.Contains("新建"));

                var cases = queryable.Where(x => x.BasicPatientId == id);


                if (sick == null)
                {
                    return ApiResult<ApiResult>.Fail("未找到该患者的病历记录", ResultCode.NotFound);
                }

                // 如果状态为已登记或住院中，不允许再次入院
                if (sick.Status == HospitalizationStatus.Registered.ToString() ||
                    sick.Status == HospitalizationStatus.Discharged.ToString())
                {
                    return ApiResult<ApiResult>.Fail("该患者已登记或正在住院中，无法重复入院", ResultCode.Error);
                }

                // 仅允许新建状态的入院
                if (sick.Status != "新建")
                {
                    return ApiResult<ApiResult>.Fail("病历状态不是新建，无法执行入院操作", ResultCode.Error);
                }

                // 修改状态为住院中
                sick.Status = HospitalizationStatus.Registered.ToString();
                await _sickRepository.UpdateAsync(sick);
                return ApiResult<ApiResult>.Success(
                    ApiResult.Success(ResultCode.Success)
                    , ResultCode.Success);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 获取所有住院记录(下拉)
        /// </summary>
        public async Task<ApiResult<List<GetAllSickInfoDto>>> GetAllPatient()
        {
            var list = await _sickRepository.GetQueryableAsync();
            list = list.Where(x => x.Status == "新建");
            var result = new List<GetAllSickInfoDto>();
            foreach (var sick in list)
            {
                result.Add(new GetAllSickInfoDto()
                {
                    Id = sick.BasicPatientId,
                    PatientName = _patientRepository.GetAsync(sick.BasicPatientId).Result.PatientName ?? "",
                });
            }
            return ApiResult<List<GetAllSickInfoDto>>.Success(result, ResultCode.Success);
        }

        /// <summary>
        /// 获取所有住院记录
        /// </summary>
        public async Task<ApiResult<List<HospitalizedDto>>> GetListAsync(string keyword)
        {
            var list = await (await _sickRepository.GetQueryableAsync())
                .Where(x => x.Status != "新建" && x.Status == keyword)
                .ToListAsync();


            var dtoList = ObjectMapper.Map<List<Sick>, List<HospitalizedDto>>(list);
            return ApiResult<List<HospitalizedDto>>.Success(dtoList, ResultCode.Success);
        }

        /// <summary>
        /// 获取单条住院记录
        /// </summary>
        public async Task<ApiResult<HospitalizedDto>> GetAsync(Guid id)
        {
            var entity = await _sickRepository.GetAsync(id);
            var dto = ObjectMapper.Map<Sick, HospitalizedDto>(entity);
            return ApiResult<HospitalizedDto>.Success(dto, ResultCode.Success);
        }

        /// <summary>
        /// 更新住院记录
        /// </summary>
        public async Task<ApiResult> UpdateAsync(Guid id, CreateUpdateHospitalizedDto input)
        {
            var entity = await _sickRepository.GetAsync(id);
            ObjectMapper.Map(input, entity);
            await _sickRepository.UpdateAsync(entity);
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 更改住院状态
        /// </summary>
        /// <param name="id">病历记录ID</param>
        /// <param name="status">新状态</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<ApiResult>> UpdateStatusAsync(Guid id, string status)
        {
            var sick = await _sickRepository.GetAsync(id);
            if (sick == null)
            {
                return ApiResult<ApiResult>.Fail("未找到该患者的病历记录", ResultCode.NotFound);
            }
            sick.Status = status;
            await _sickRepository.UpdateAsync(sick);
            return ApiResult<ApiResult>.Success(
                    ApiResult.Success(ResultCode.Success)
                    , ResultCode.Success);
        }

        /// <summary>
        /// 删除住院记录
        /// </summary>
        public async Task<ApiResult> DeleteAsync(Guid id)
        {
            await _sickRepository.DeleteAsync(id);
            return ApiResult.Success(ResultCode.Success);
        }


        #endregion

        [HttpPost]
        public async Task<ApiResult<OutpatientClinic.Dtos.Parameter.PagedResultDto<RegistrationListDto>>> GetRegistrationsAsync(GetRegistrationsInput input)
        {
            try
            {
                var clinicQueryable = (await _clinicRepository.GetQueryableAsync()).AsNoTracking();
                var patientQueryable = (await _patientRepository.GetQueryableAsync()).AsNoTracking();
                var doctorQueryable = (await _doctorRepository.GetQueryableAsync()).AsNoTracking();

                var query = from clinic in clinicQueryable
                            join patient in patientQueryable on clinic.PatientId equals patient.Id
                            join doctor in doctorQueryable on clinic.DoctorId equals doctor.Id
                            select new { clinic, patient, doctor };

                query = query
                    .WhereIf(input.StartTime.HasValue, q => q.clinic.VisitDateTime >= input.StartTime.Value)
                    .WhereIf(input.EndTime.HasValue, q => q.clinic.VisitDateTime <= input.EndTime.Value)
                    .WhereIf(!input.DepartmentName.IsNullOrWhiteSpace(), q => q.clinic.DepartmentName == input.DepartmentName)
                    .WhereIf(input.DoctorId.HasValue, q => q.doctor.Id == input.DoctorId.Value)
                    .WhereIf(!string.IsNullOrEmpty(input.PatientName), q => q.patient.PatientName.Contains(input.PatientName))
                    .WhereIf(!input.Status.IsNullOrWhiteSpace(), q => q.clinic.ExecutionStatus == Enum.Parse<ExecutionStatus>(input.Status));

                var totalCount = await AsyncExecuter.CountAsync(query);

                query = query.OrderByDescending(q => q.clinic.Id);

                var queryPaged = query
                        .OrderByDescending(q => q.clinic.Id)
                        .PageBy(input);

                var dtoList = query.Select(q => new RegistrationListDto
                {
                    Id = q.clinic.Id,
                    DepartmentName = q.clinic.DepartmentName,
                    PatientName = q.patient.PatientName,
                    DoctorName = q.doctor.EmployeeName,
                    VisitType = q.clinic.VisitType,
                    Status = q.clinic.ExecutionStatus.ToString(),
                    Remarks = q.clinic.Remarks,
                    VisitId = q.patient.VisitId,
                    Gender = q.patient.Gender,
                    Age = q.patient.Age,
                    AgeUnit = q.patient.AgeUnit,
                    ContactPhone = q.patient.ContactPhone,
                    IdNumber = q.patient.IdNumber,
                    IsInfectiousDisease = q.patient.IsInfectiousDisease,
                    DiseaseOnsetTime = q.patient.DiseaseOnsetTime,
                    EmergencyTime = q.patient.EmergencyTime,
                    PatientVisitStatus = q.patient.VisitStatus,
                    PatientVisitDate = q.patient.VisitDate
                }).ToList();

                return ApiResult<OutpatientClinic.Dtos.Parameter.PagedResultDto<RegistrationListDto>>.Success(new OutpatientClinic.Dtos.Parameter.PagedResultDto<RegistrationListDto>(totalCount, dtoList), ResultCode.Success);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
