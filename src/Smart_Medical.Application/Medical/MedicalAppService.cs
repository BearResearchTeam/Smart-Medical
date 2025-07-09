using Microsoft.AspNetCore.Mvc;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Medical
{
    /// <summary>
    /// 医疗管理AppService
    /// </summary>
    [ApiExplorerSettings(GroupName = "医疗管理")]
    public class MedicalAppService : ApplicationService, IMedicalAppService
    {
        private readonly IRepository<Sick, Guid> _repository;

        public MedicalAppService(IRepository<Sick, Guid> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取单个病历信息
        /// </summary>
        /// <param name="id">病历Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<SickDto>> GetAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetAsync(id);
                if (entity == null)
                {
                    throw new UserFriendlyException("病历不存在！");
                }
                var dto = ObjectMapper.Map<Sick, SickDto>(entity);
                return ApiResult<SickDto>.Success(dto, ResultCode.Success);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 分页获取病历列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PagedResultDto<SickDto>>> GetListAsync([FromQuery] SickSearchDto search)
        {
            var list = await _repository.GetQueryableAsync();
            list = list.WhereIf(!string.IsNullOrWhiteSpace(search.PatientName), x => x.PatientName.Contains(search.PatientName))
                       .WhereIf(!string.IsNullOrWhiteSpace(search.InpatientNumber), x => x.InpatientNumber.Contains(search.InpatientNumber))
                       .WhereIf(!string.IsNullOrWhiteSpace(search.AdmissionDiagnosis), x => x.AdmissionDiagnosis.Contains(search.AdmissionDiagnosis));

            var totalCount = await AsyncExecuter.CountAsync(list);
            var items = await AsyncExecuter.ToListAsync(
                list
                .Skip((search.pageIndex - 1) * search.pageSize)
                    .Take(search.pageSize)
            );

            var dtos = ObjectMapper.Map<List<Sick>, List<SickDto>>(items);
            var result = new PagedResultDto<SickDto>(totalCount, dtos);

            return ApiResult<PagedResultDto<SickDto>>.Success(result, ResultCode.Success);
        }

        /// <summary>
        /// 添加病历
        /// </summary>
        /// <param name="input">病历信息</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> CreateAsync(CreateUpdateSickDto input)
        {
            // 验证住院号唯一性
            var exists = await _repository.AnyAsync(s => s.InpatientNumber == input.InpatientNumber);
            if (exists)
            {
                throw new UserFriendlyException($"住院号 '{input.InpatientNumber}' 已存在！");
            }

            // 验证体温范围
            if (input.Temperature < 30 || input.Temperature > 45)
            {
                throw new UserFriendlyException("体温必须在30~45℃之间！");
            }

            // 验证脉搏范围
            if (input.Pulse < 20 || input.Pulse > 200)
            {
                throw new UserFriendlyException("脉搏必须在20~200次/min之间！");
            }

            // 验证呼吸范围
            if (input.Breath < 5 || input.Breath > 60)
            {
                throw new UserFriendlyException("呼吸必须在5~60次/min之间！");
            }


            var entity = ObjectMapper.Map<CreateUpdateSickDto, Sick>(input);
            await _repository.InsertAsync(entity);

            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 更新病历信息
        /// </summary>
        /// <param name="id">病历Id</param>
        /// <param name="input">更新参数</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> UpdateAsync(Guid id, CreateUpdateSickDto input)
        {
            var entity = await _repository.GetAsync(id);

            // 验证体温范围
            if (input.Temperature < 30 || input.Temperature > 45)
            {
                throw new UserFriendlyException("体温必须在30~45℃之间！");
            }

            // 验证脉搏范围
            if (input.Pulse < 20 || input.Pulse > 200)
            {
                throw new UserFriendlyException("脉搏必须在20~200次/min之间！");
            }

            // 验证呼吸范围
            if (input.Breath < 5 || input.Breath > 60)
            {
                throw new UserFriendlyException("呼吸必须在5~60次/min之间！");
            }

            // 验证出院时间：只有当状态不是"出院"时，才要求出院时间不能早于当前
            if (input.Status != "出院" && input.DischargeTime < DateTime.Now)
            {
                throw new UserFriendlyException("对于非出院状态的病人，出院时间不能早于当前时间！");
            }

            ObjectMapper.Map(input, entity);
            await _repository.UpdateAsync(entity);

            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 删除病历
        /// </summary>
        /// <param name="id">病历Id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<ApiResult> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetAsync(id);
            if (entity == null)
            {
                throw new UserFriendlyException("病历不存在！");
            }

            await _repository.DeleteAsync(id);
            return ApiResult.Success(ResultCode.Success);
        }




    }
}
