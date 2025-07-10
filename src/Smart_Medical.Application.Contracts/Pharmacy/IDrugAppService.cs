using Smart_Medical.Pharmacy;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Smart_Medical.Pharmacy
{
    /// <summary>
    /// 药品服务接口
    /// </summary>
    public interface IDrugAppService : IApplicationService
    {

 

        Task<ApiResult<DrugDto>> GetAsync(int id);
        Task<ApiResult<PageResult<List<DrugDto>>>> GetListAsync([FromQuery] DrugSearchDto input);
        Task<ApiResult> CreateAsync(CreateUpdateDrugDto input);
        Task<ApiResult> UpdateAsync(int id, CreateUpdateDrugDto input);
        Task<ApiResult> DeleteAsync(int id);

        Task<ApiResult<List<DrugGroupByTypeDto>>> GetDrugsGroupedByTypeAsync();
    }
}
