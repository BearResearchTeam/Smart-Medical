using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Smart_Medical.Prescriptions
{
    /// <summary>
    /// 处方管理服务接口
    /// </summary>
    public interface IPrescriptionService : IApplicationService
    {
        Task<ApiResult> CreateAsync(PrescriptionDto input);
        Task<ApiResult<List<PrescriptionTree>>> GetPrescriptionTree(int pid);
        Task<ApiResult<List<GetPrescriptionDrugDto>>> GetPrescriptionTreeList(int? prescriptionid);
    }
}
