using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Smart_Medical.Until;

namespace Smart_Medical.RBAC.RolePermissions
{
    /// <summary>
    /// 角色权限关联接口服务
    /// </summary>
    public interface IRolePermissionAppService : IApplicationService
    {
        Task<ApiResult> BatchCreateAsync(Guid roleId, List<Guid> permissionIds);
        Task<ApiResult<RolePermissionDto>> GetAsync(Guid id);

        Task<ApiResult<PageResult<List<RolePermissionDto>>>> GetListAsync(SeachRolePermissionDto input);

        Task<ApiResult> CreateAsync(CreateUpdateRolePermissionDto input);

        Task<ApiResult> DeleteAsync(Guid id);
    }
}