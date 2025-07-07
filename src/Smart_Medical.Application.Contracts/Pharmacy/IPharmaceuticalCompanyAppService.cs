using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Smart_Medical.Until;

namespace Smart_Medical.Pharmacy
{
    /// <summary>
    /// 制药公司服务接口
    /// </summary>
    public interface IPharmaceuticalCompanyAppService : IApplicationService
    {
        /// <summary>
        /// 根据公司名称查询
        /// </summary>
        /// <param name="name">公司名称</param>
        /// <returns></returns>
        Task<ApiResult> FindByNameAsync(string name);

        /// <summary>
        /// 获取所有公司列表
        /// </summary>
        /// <returns></returns>
        Task<ApiResult> GetListAllAsync();

        /// <summary>
        /// 新增制药公司
        /// </summary>
        /// <param name="input">制药公司信息</param>
        /// <returns></returns>
        Task<ApiResult> CreateAsync(CreateUpdatePharmaceuticalCompanyDto input);
  
    }
}
