using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Smart_Medical.Until;

namespace Smart_Medical.RBAC.UserRoles
{
    /// <summary>
    /// 用户角色关联接口服务
    /// </summary>
    public interface IUserRoleAppService : IApplicationService
    {

        Task<ApiResult<UserRoleDto>> GetAsync(Guid id);

        Task<ApiResult<PageResult<List<UserRoleDto>>>> GetListAsync(SeachUserRoleDto input);

        Task<ApiResult> CreateAsync(CreateUpdateUserRoleDto input);

        Task<ApiResult> DeleteAsync(Guid id);

        // 更新用户角色关联的方法。这里更新的语义是：为指定的用户设置其所有的角色。
        // 您可以选择以下两种实现方式之一：
        // 1. 先删除现有所有关联，再添加新的关联（简单直接）。
        // 2. 对比现有关联和新关联，只添加新增的，删除已移除的（更高效）。
        Task<ApiResult> UpdateAsync(Guid userId, List<Guid> roleIds);
        Task<ApiResult> BatchCreateAsync(Guid userId, List<Guid> roleIds);
    }
}