using Smart_Medical.OutpatientClinic.Dtos;
using Smart_Medical.Registration.Dtos;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Smart_Medical.Registration
{
    public interface IHospitalizedService : IApplicationService
    {
        Task<ApiResult<ApiResult>> CreateAsync(Guid id);
        Task<ApiResult> DeleteAsync(Guid id);
        Task<ApiResult<List<GetAllSickInfoDto>>> GetAllPatient();
        Task<ApiResult<HospitalizedDto>> GetAsync(Guid id);
        Task<ApiResult<OutpatientClinic.Dtos.Parameter.PagedResultDto<RegistrationListDto>>> GetRegistrationsAsync(GetRegistrationsInput input);
        Task<ApiResult> UpdateAsync(Guid id, CreateUpdateHospitalizedDto input);
        Task<ApiResult<ApiResult>> UpdateStatusAsync(Guid id, string status);
    }
}