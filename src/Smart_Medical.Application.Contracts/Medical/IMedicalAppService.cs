using Smart_Medical.Application.Contracts.Medical;
using Smart_Medical.Medical.Smart_Medical.Medical;
using Smart_Medical.Until;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Smart_Medical.Medical
{
    /// <summary>
    /// 病历管理服务接口
    /// </summary>
    public interface IMedicalAppService : IApplicationService
    {
     
        /// <summary>
        /// 分页获取病历全信息（含患者、就诊、处方、药品、挂号等）
        /// </summary>
        /// <param name="input">查询条件（可包含姓名、住院号、分页参数等）</param>
        /// <returns>分页结果（ApiResult包装）</returns>
        Task<ApiResult<PagedResultDto<SickFullInfoDto>>> GetFullInfoPagedAsync(SickSearchDto input);

       
    }
}