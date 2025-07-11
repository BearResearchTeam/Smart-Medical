using Microsoft.AspNetCore.Mvc;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Smart_Medical.Medical
{
    /// <summary>
    /// 病种管理服务接口
    /// </summary>
    public interface IMedicalAppService : IApplicationService
    {
        /// <summary>
        /// 获取单个疾病信息
        /// </summary>
        /// <param name="id">疾病Id</param>
        /// <returns></returns>
        Task<ApiResult<SickDto>> GetAsync(Guid id);
        /// <summary>
        /// 分页获取疾病列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns></returns>
        Task<ApiResult<PagedResultDto<SickDto>>> GetListAsync([FromQuery] SickSearchDto input);
        /// <summary>
        /// 创建疾病信息
        /// </summary>
        /// <param name="input">创建参数</param>
        /// <returns></returns>
        Task<ApiResult> CreateAsync(CreateUpdateSickDto input);
        /// <summary>
        /// 更新疾病信息
        /// </summary>
        /// <param name="id">疾病Id</param>
        /// <param name="input">更新参数</param>
        /// <returns></returns>
        Task<ApiResult> UpdateAsync(Guid id, CreateUpdateSickDto input);
        /// <summary>
        /// 删除疾病信息
        /// </summary>
        /// <param name="id">疾病Id</param>
        /// <returns></returns>
        Task<ApiResult> DeleteAsync(Guid id);
    }
}
