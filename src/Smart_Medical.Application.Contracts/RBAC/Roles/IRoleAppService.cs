﻿using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Smart_Medical.Application.Contracts.RBAC.Roles
{
    /// <summary>
    /// 角色接口服务
    /// </summary>
    public interface IRoleAppService : IApplicationService
    {
        Task<ApiResult<RoleDto>> GetAsync(Guid id);

        Task<ApiResult<PageResult<List<RoleDto>>>> GetListAsync(SeachRoleDto input);

        Task<ApiResult> CreateAsync(CreateUpdateRoleDto input);

        Task<ApiResult> UpdateAsync(Guid id, CreateUpdateRoleDto input);

        Task<ApiResult> DeleteAsync(Guid id);
    }
}
