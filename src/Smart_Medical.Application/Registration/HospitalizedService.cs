using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Medical.Medical;
using Smart_Medical.OutpatientClinic.Dtos;
using Smart_Medical.OutpatientClinic.IServices;
using Smart_Medical.Patient;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public HospitalizedService(IRepository<Sick, Guid> sickRepository, IRepository<BasicPatientInfo, Guid> patientRepository)
        {
            _sickRepository = sickRepository;
            _patientRepository = patientRepository;
        }

        /// <summary>
        /// 办理入院
        /// </summary>
        public async Task<ApiResult<ApiResult>> CreateAsync(Guid id)
        {
            try
            {
                var sick = await _sickRepository.FirstOrDefaultAsync(x => x.Status == "新建" && x.BasicPatientId == id);
                if (sick != null)
                {
                    return ApiResult<ApiResult>.Fail("该患者已住院", ResultCode.Error);
                }
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
            var list = await _sickRepository.GetListAsync();
            var result = new List<GetAllSickInfoDto>();
            foreach (var sick in list)
            {
                result.Add(new GetAllSickInfoDto()
                {
                    Id = sick.Id,
                    PatientName = _patientRepository.GetAsync(sick.BasicPatientId).Result.PatientName,
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
                .Where(x => x.Status != "新建")
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
        /// 删除住院记录
        /// </summary>
        public async Task<ApiResult> DeleteAsync(Guid id)
        {
            await _sickRepository.DeleteAsync(id);
            return ApiResult.Success(ResultCode.Success);
        }
    }
}
