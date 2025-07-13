using Smart_Medical.Equipment.Dtos;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Smart_Medical.Equipment
{
    public interface IEquipmentAppService : IApplicationService
    {
        Task<ApiResult<EquipmentManagementDto>> GetAsync(Guid id);
        Task<ApiResult<PagedResultDto<EquipmentManagementDto>>> GetListAsync(GeEquipmentParameterDtos input);
        Task<ApiResult<ApiResult>> CreateAsync(CreateUpdateEquipmentManagementDto input);
        Task<ApiResult> UpdateAsync(Guid id, CreateUpdateEquipmentManagementDto input);
        Task<ApiResult> DeleteAsync(Guid id);
    }
} 