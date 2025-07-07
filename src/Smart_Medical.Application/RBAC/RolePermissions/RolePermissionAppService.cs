using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Smart_Medical.Until;
using Smart_Medical.RBAC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Medical;
using Volo.Abp.Uow;
using Volo.Abp.DependencyInjection;

namespace Smart_Medical.RBAC.RolePermissions
{
    /// <summary>
    /// 角色与权限的关联服务
    /// </summary>
    [ApiExplorerSettings(GroupName = "角色权限关联管理")]
    [Dependency(ReplaceServices = true)]
    public class RolePermissionAppService : ApplicationService, IRolePermissionAppService
    {
        private readonly IRepository<RolePermission, Guid> _rolePermissionRepository;
        private readonly IRepository<Role, Guid> _roleRepository; // 用于获取角色详情
        private readonly IRepository<Permission, Guid> _permissionRepository; // 用于获取权限详情

        public RolePermissionAppService(IRepository<RolePermission, Guid> rolePermissionRepository,
                                        IRepository<Role, Guid> roleRepository,
                                        IRepository<Permission, Guid> permissionRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
        }

        /// <summary>
        /// 为指定角色授予一个新权限
        /// </summary>
        /// <param name="input">包含角色ID和权限ID的数据传输对象</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> CreateAsync(CreateUpdateRolePermissionDto input)
        {
            // 检查角色和权限是否存在
            var roleExists = await _roleRepository.AnyAsync(r => r.Id == input.RoleId);
            if (!roleExists)
            {
                return ApiResult.Fail("角色不存在", ResultCode.NotFound);
            }
            var permissionExists = await _permissionRepository.AnyAsync(p => p.Id == input.PermissionId);
            if (!permissionExists)
            {
                return ApiResult.Fail("权限不存在", ResultCode.NotFound);
            }

            // 检查是否已存在相同的角色权限关联
            var existingRolePermission = await _rolePermissionRepository.FirstOrDefaultAsync(
                rp => rp.RoleId == input.RoleId && rp.PermissionId == input.PermissionId
            );
            if (existingRolePermission != null)
            {
                return ApiResult.Fail("该角色已拥有此权限", ResultCode.AlreadyExists);
            }

            var rolePermission = ObjectMapper.Map<CreateUpdateRolePermissionDto, RolePermission>(input);
            await _rolePermissionRepository.InsertAsync(rolePermission);
            return ApiResult.Success(ResultCode.Success);
        }


        /// <summary>
        /// 为指定角色批量授予权限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionIds">权限ID列表</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> BatchCreateAsync(Guid roleId, List<Guid> permissionIds)
        {
            // 1. 检查角色是否存在
            var roleExists = await _roleRepository.AnyAsync(r => r.Id == roleId);
            if (!roleExists)
            {
                return ApiResult.Fail("角色不存在", ResultCode.NotFound);
            }

            // 2. 检查所有权限是否存在
            foreach (var permissionId in permissionIds)
            {
                var permissionExists = await _permissionRepository.AnyAsync(p => p.Id == permissionId);
                if (!permissionExists)
                {
                    return ApiResult.Fail($"权限ID {permissionId} 不存在", ResultCode.NotFound);
                }
            }

            // 3. 查询已存在的关联，避免重复插入
            var existings = (await _rolePermissionRepository.GetQueryableAsync())
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .Select(rp => rp.PermissionId)
                .ToList();

            var toAdd = permissionIds.Except(existings).ToList();

            foreach (var permissionId in toAdd)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                };
                await _rolePermissionRepository.InsertAsync(rolePermission);
            }

            return ApiResult.Success(ResultCode.Success);
        }


        /// <summary>
        /// 根据ID删除一条角色权限的关联记录
        /// </summary>
        /// <param name="id">角色权限关联记录的ID</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> DeleteAsync(Guid id)
        {
            var rolePermission = await _rolePermissionRepository.GetAsync(id);
            if (rolePermission == null)
            {
                return ApiResult.Fail("角色权限关联不存在", ResultCode.NotFound);
            }
            await _rolePermissionRepository.DeleteAsync(rolePermission);
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 根据ID获取单条角色权限关联记录的详细信息
        /// </summary>
        /// <param name="id">角色权限关联记录的ID</param>
        /// <returns>包含角色权限关联详细信息的ApiResult</returns>
        public async Task<ApiResult<RolePermissionDto>> GetAsync(Guid id)
        {
            // 使用 Include 联查 Role 和 Permission 实体
            var rolePermission = await _rolePermissionRepository.WithDetailsAsync(rp => rp.Role, rp => rp.Permission);

            var result = rolePermission.FirstOrDefault(rp => rp.Id == id);

            if (result == null)
            {
                return ApiResult<RolePermissionDto>.Fail("角色权限关联不存在", ResultCode.NotFound);
            }
            var rolePermissionDto = ObjectMapper.Map<RolePermission, RolePermissionDto>(result);
            return ApiResult<RolePermissionDto>.Success(rolePermissionDto, ResultCode.Success);
        }

        /// <summary>
        /// 根据查询条件分页获取角色权限关联列表
        /// </summary>
        /// <param name="input">包含分页和筛选信息的查询DTO</param>
        /// <returns>包含角色权限关联列表和分页信息的ApiResult</returns>
        public async Task<ApiResult<PageResult<List<RolePermissionDto>>>> GetListAsync([FromQuery] SeachRolePermissionDto input)
        {
            var queryable = await _rolePermissionRepository.GetQueryableAsync();

            // 使用 Include 联查 Role 和 Permission 实体，确保在映射到 DTO 时包含关联数据
            queryable = queryable.Include(rp => rp.Role).Include(rp => rp.Permission);

            if (input.RoleId.HasValue)
            {
                queryable = queryable.Where(rp => rp.RoleId == input.RoleId.Value);
            }
            if (input.PermissionId.HasValue)
            {
                queryable = queryable.Where(rp => rp.PermissionId == input.PermissionId.Value);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .OrderBy(rp => rp.RoleId); // 默认排序

            var rolePermissions = await AsyncExecuter.ToListAsync(queryable);
            var rolePermissionDtos = ObjectMapper.Map<List<RolePermission>, List<RolePermissionDto>>(rolePermissions);

            var pageResult = new PageResult<List<RolePermissionDto>>
            {
                TotleCount = totalCount,
                TotlePage = (int)Math.Ceiling((double)totalCount / input.MaxResultCount),
                Data = rolePermissionDtos
            };

            return ApiResult<PageResult<List<RolePermissionDto>>>.Success(pageResult, ResultCode.Success);
        }

        // UpdateAsync 方法对于中间表通常不需要，因为关联的修改通常是删除旧关联再创建新关联。
        // 如果需要修改 RoleId 或 PermissionId，通常会先删除再新增。
        // 此处省略 UpdateAsync，如果确实需要，可以根据业务逻辑添加。

        /// <summary>
        /// 批量更新指定角色所拥有的权限
        /// </summary>
        /// <remarks>
        /// 此方法会同步角色权限。传入的权限ID列表将成为该角色的全部权限。
        /// - 如果角色已有关联，但不存在于传入列表中，该关联将被软删除。
        /// - 如果传入列表中的权限关联尚不存在，将被新增。
        /// - 如果传入列表中的权限关联过去存在但被软删了，将被恢复。
        /// </remarks>
        /// <param name="roleId">要更新权限的角色ID</param>
        /// <param name="newPermissionIds">该角色应拥有的所有权限的ID列表</param>
        /// <returns>操作结果</returns>
        [UnitOfWork] // 添加 [UnitOfWork] 特性，确保此方法的数据库操作在事务中执行
        public async Task<ApiResult> UpdateAsync(Guid roleId, List<Guid> newPermissionIds)
        {
            // 1. 验证角色是否存在
            var roleExists = await _roleRepository.AnyAsync(r => r.Id == roleId);
            if (!roleExists)
            {
                return ApiResult.Fail("角色不存在", ResultCode.NotFound);
            }

            // 2. 验证所有传入的权限ID是否存在
            foreach (var permissionId in newPermissionIds)
            {
                var permissionExists = await _permissionRepository.AnyAsync(p => p.Id == permissionId);
                if (!permissionExists)
                {
                    return ApiResult.Fail($"权限ID {permissionId} 不存在", ResultCode.NotFound);
                }
            }

            // 获取角色 当前权限
            var currentRolePermissions = (await _rolePermissionRepository.GetQueryableAsync()).Where(rp => rp.RoleId == roleId).ToList();
            //只拿权限id 
            var currentPermissionIds = currentRolePermissions.Select(rp => rp.PermissionId).ToList();

            // 逻辑步骤3：找出需要新增的权限ID (新列表有，但当前没有)
            var permissionsToAdd = newPermissionIds.Except(currentPermissionIds).ToList();

            // 逻辑步骤4：找出需要删除的权限ID (当前有，但新列表没有)
            var permissionsToRemove = currentPermissionIds.Except(newPermissionIds).ToList();

            // 逻辑步骤5：执行删除
            foreach (var permissionId in permissionsToRemove)
            {
                var rolePermissionToRemove = currentRolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
                if (rolePermissionToRemove != null)
                {
                    await _rolePermissionRepository.DeleteAsync(rolePermissionToRemove); // This performs soft delete
                }
            }

            // 逻辑步骤6：执行新增或恢复
            foreach (var permissionId in permissionsToAdd)
            {
                // 查找是否存在已存在的（包括软删除的）记录
                var queryableRolePermissions = await _rolePermissionRepository.GetQueryableAsync();

                var existingRolePermission = await queryableRolePermissions.AsNoTracking().IgnoreQueryFilters()
                                                                            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

                if (existingRolePermission != null)
                {
                    // 如果存在且是软删除状态，则恢复
                    if (existingRolePermission.IsDeleted) // Assuming RolePermission implements ISoftDelete
                    {
                        existingRolePermission.IsDeleted = false;
                        existingRolePermission.DeletionTime = null; // 修正：existingRole -> existingRolePermission
                        await _rolePermissionRepository.UpdateAsync(existingRolePermission);
                    }
                    // 如果存在且不是软删除状态，则不做任何操作 (理论上不应该进入这个分支)
                }
                else
                {
                    // 不存在任何记录（包括软删除），则新增
                    var newRolePermission = new RolePermission(); // 使用无参构造函数
                    newRolePermission.RoleId = roleId;
                    newRolePermission.PermissionId = permissionId;
                    await _rolePermissionRepository.InsertAsync(newRolePermission);
                }
            }

            return ApiResult.Success(ResultCode.Success);
        }
    }
}