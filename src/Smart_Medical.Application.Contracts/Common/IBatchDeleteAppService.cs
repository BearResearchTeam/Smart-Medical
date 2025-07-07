using Microsoft.AspNetCore.Mvc;
using Smart_Medical.Pharmacy;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Smart_Medical.Common
{
    /// <summary>
    /// 批量删除应用服务接口
    /// </summary>
    public interface IBatchDeleteAppService : IApplicationService
    {
        /// <summary>
        /// 批量删除药品（带事务）
        /// </summary>
        /// <param name="input">批量删除参数</param>
        /// <returns></returns>
        Task<ApiResult> BatchDeleteDrugsAsync(BatchDeleteDrugDto input);

       
    }
} 