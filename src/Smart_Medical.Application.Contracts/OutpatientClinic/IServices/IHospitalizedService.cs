using Smart_Medical.OutpatientClinic.Dtos;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smart_Medical.Registration
{
    public interface IHospitalizedService
    {
        Task<ApiResult<ApiResult>> CreateAsync(Guid id);
        Task<ApiResult> DeleteAsync(Guid id);
        Task<ApiResult<List<GetAllSickInfoDto>>> GetAllPatient();
        Task<ApiResult<HospitalizedDto>> GetAsync(Guid id);
        Task<ApiResult<List<HospitalizedDto>>> GetListAsync();
        Task<ApiResult> UpdateAsync(Guid id, CreateUpdateHospitalizedDto input);
    }
}