using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Smart_Medical.Until;
using Microsoft.AspNetCore.Mvc;

namespace Smart_Medical.DoctorvVsit.DoctorAccounts
{
    /// <summary>
    /// 医生账户服务接口
    /// </summary>
    public interface IDoctorAccountSerivce: IApplicationService
    {
        /// <summary>
        /// 添加医生账户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<ApiResult> InsertDoctorAccount(CreateUpdateDoctorAccountDto input);
        /// <summary>
        /// 获取医生账户列表(分页)
        /// </summary>
        Task<ApiResult<PageResult<List<DoctorAccountListDto>>>> GetDoctorAccountList(DoctorAccountsearch seach);
        /// <summary>
        /// 修改医生账户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<ApiResult> EditDoctorAccount(Guid id, CreateUpdateDoctorAccountDto input);
        /// <summary>
        /// 获取医生账户列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApiResult> DeleteDoctorAccount([FromQuery] string idsString);
    }
}
