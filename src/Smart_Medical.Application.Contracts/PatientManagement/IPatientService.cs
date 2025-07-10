using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Smart_Medical.PatientManagement
{
    public interface IPatientService : IApplicationService
    {
        Task<ApiResult<PageResultDto<GetInsertPatientDto>>> GetPatientInfoAsync(GetVistingParameterDtos input);

        Task<ApiResult> DeletePatientInfo(Guid PatId);

        Task<ApiResult> UpdatePatientInfo(UpdPatientDto updPatientDto);
    }
}
