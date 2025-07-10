using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Medical.RBAC.Roles;
using Smart_Medical.RBAC.Users;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Smart_Medical.RBAC.UserRoles
{
    /// <summary>
    /// 用户与角色的关联服务
    /// </summary>
    [ApiExplorerSettings(GroupName = "用户角色关联管理")]
    [Dependency(ReplaceServices = true)]
    //[Authorize]
    public class UserRoleAppService : ApplicationService, IUserRoleAppService
    {
        private readonly IRepository<UserRole, Guid> _userRoleRepository;
        private readonly IRepository<User, Guid> _userRepository; // 用于获取用户详情
        private readonly IRepository<Role, Guid> _roleRepository; // 用于获取角色详情

        public UserRoleAppService(IRepository<UserRole, Guid> userRoleRepository,
                                  IRepository<User, Guid> userRepository,
                                  IRepository<Role, Guid> roleRepository)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// 为指定用户授予一个新角色
        /// </summary>
        /// <param name="input">包含用户ID和角色ID的数据传输对象</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> CreateAsync(CreateUpdateUserRoleDto input)
        {
            // 检查用户和角色是否存在
            var userExists = await _userRepository.AnyAsync(u => u.Id == input.UserId);
            if (!userExists)
            {
                return ApiResult.Fail("用户不存在", ResultCode.NotFound);
            }
            var roleExists = await _roleRepository.AnyAsync(r => r.Id == input.RoleId);
            if (!roleExists)
            {
                return ApiResult.Fail("角色不存在", ResultCode.NotFound);
            }

            // 检查是否已存在相同的用户角色关联
            var existingUserRole = await _userRoleRepository.FirstOrDefaultAsync(
                ur => ur.UserId == input.UserId && ur.RoleId == input.RoleId
            );
            if (existingUserRole != null)
            {
                return ApiResult.Fail("该用户已拥有此角色", ResultCode.AlreadyExists);
            }

            var userRole = ObjectMapper.Map<CreateUpdateUserRoleDto, UserRole>(input);
            await _userRoleRepository.InsertAsync(userRole);
            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 为指定用户批量授予新角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleIds">要授予的角色ID列表</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> BatchCreateAsync(Guid userId, List<Guid> roleIds)
        {
            // 检查用户是否存在
            var userExists = await _userRepository.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return ApiResult.Fail("用户不存在", ResultCode.NotFound);
            }

            // 检查所有传入的角色是否存在
            foreach (var roleId in roleIds)
            {
                var roleExists = await _roleRepository.AnyAsync(r => r.Id == roleId);
                if (!roleExists)
                {
                    return ApiResult.Fail($"角色ID {roleId} 不存在", ResultCode.NotFound);
                }
            }

            // 获取该用户已拥有的角色关联，避免重复添加
            var existingRoleIds = (await _userRoleRepository.GetQueryableAsync())
                                    .Where(ur => ur.UserId == userId && roleIds.Contains(ur.RoleId))
                                    .Select(ur => ur.RoleId)
                                    .ToList();

            // 过滤出需要新增的角色ID
            var newRoleIds = roleIds.Except(existingRoleIds).ToList();

            // 批量新增角色关联
            foreach (var roleId in newRoleIds)
            {
                var userRole = new UserRole(GuidGenerator.Create())
                {
                    UserId = userId,
                    RoleId = roleId
                };
                await _userRoleRepository.InsertAsync(userRole);
            }

            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 根据ID删除一条用户角色的关联记录
        /// </summary>
        /// <param name="id">用户角色关联记录的ID</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> DeleteAsync(Guid id)
        {
            var userRole = await _userRoleRepository.GetAsync(id);
            if (userRole == null)
            {
                return ApiResult.Fail("用户角色关联不存在", ResultCode.NotFound);
            }
            await _userRoleRepository.DeleteAsync(userRole);
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 根据ID获取单条用户角色关联记录的详细信息
        /// </summary>
        /// <param name="id">用户角色关联记录的ID</param>
        /// <returns>包含用户角色关联详细信息的ApiResult</returns>
        public async Task<ApiResult<UserRoleDto>> GetAsync(Guid id)
        {
            // 使用 Include 联查 User 和 Role 实体
            var userRole = await _userRoleRepository.WithDetailsAsync(ur => ur.User, ur => ur.Role);

            var result = userRole.FirstOrDefault(ur => ur.Id == id);

            if (result == null)
            {
                return ApiResult<UserRoleDto>.Fail("用户角色关联不存在", ResultCode.NotFound);
            }
            var userRoleDto = ObjectMapper.Map<UserRole, UserRoleDto>(result);
            return ApiResult<UserRoleDto>.Success(userRoleDto, ResultCode.Success);
        }

        /// <summary>
        /// 根据查询条件分页获取用户角色关联列表
        /// </summary>
        /// <param name="input">包含分页和筛选信息的查询DTO</param>
        /// <returns>包含用户角色关联列表和分页信息的ApiResult</returns>
        public async Task<ApiResult<PageResult<List<UserRoleDto>>>> GetListAsync([FromQuery] SeachUserRoleDto input)
        {
            var queryable = await _userRoleRepository.GetQueryableAsync();

            // 使用 Include 联查 User 和 Role 实体，确保在映射到 DTO 时包含关联数据
            // 在使用Select投影并需要访问导航属性时，Include是必需的。
            queryable = queryable.Include(ur => ur.User).Include(ur => ur.Role);

            if (input.UserId.HasValue)
            {
                queryable = queryable.Where(ur => ur.UserId == input.UserId.Value);
            }
            if (input.RoleId.HasValue)
            {
                queryable = queryable.Where(ur => ur.RoleId == input.RoleId.Value);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .OrderBy(ur => ur.UserId); // 默认排序

            var userRoleDtos = await AsyncExecuter.ToListAsync(
                queryable.Select(ur => new UserRoleDto
                {
                    Id = ur.Id,
                    UserId = ur.UserId,
                    RoleId = ur.RoleId,
                    CreationTime = ur.CreationTime,
                    CreatorId = ur.CreatorId,
                    LastModificationTime = ur.LastModificationTime,
                    LastModifierId = ur.LastModifierId,

                    // 因为我们已使用Include，可以断定ur.User和ur.Role在此处不为null
                    User = new UserDto
                    {
                        UserName = ur.User.UserName,
                        UserEmail = ur.User!.UserEmail,
                        UserPhone = ur.User!.UserPhone,
                        UserSex = ur.User!.UserSex,
                        // 此处的UserDto中的RoleName可以从当前UserRole关联的Role中获取
                        RoleName = new List<string> { ur.Role.RoleName }
                    },
                    Role = new RoleDto
                    {
                        Id = ur.Role!.Id,
                        RoleName = ur.Role!.RoleName,
                        Description = ur.Role!.Description,
                        CreationTime = ur.Role!.CreationTime,
                        CreatorId = ur.Role!.CreatorId,
                        LastModificationTime = ur.Role!.LastModificationTime,
                        LastModifierId = ur.Role!.LastModifierId
                    }
                })
            );

            var pageResult = new PageResult<List<UserRoleDto>>
            {
                TotleCount = totalCount,
                TotlePage = (int)Math.Ceiling((double)totalCount / input.MaxResultCount),
                Data = userRoleDtos
            };

            return ApiResult<PageResult<List<UserRoleDto>>>.Success(pageResult, ResultCode.Success);
        }

        /// <summary>
        /// 批量更新指定用户所拥有的角色
        /// </summary>
        /// <remarks>
        /// 此方法会同步用户角色。传入的角色ID列表将成为该用户的全部角色。
        /// - 如果用户已有关联，但不存在于传入列表中，该关联将被软删除。
        /// - 如果传入列表中的角色关联尚不存在，将被新增。
        /// - 如果传入列表中的角色关联过去存在但被软删了，将被恢复。
        /// </remarks>
        /// <param name="userId">要更新角色的用户ID</param>
        /// <param name="roleIds">该用户应拥有的所有角色的ID列表</param>
        /// <returns>操作结果</returns>
        [UnitOfWork] // 添加 [UnitOfWork] 特性，确保此方法的数据库操作在事务中执行
        public async Task<ApiResult> UpdateAsync(Guid userId, List<Guid> roleIds)
        {
            // 1. 验证用户是否存在
            var userExists = await _userRepository.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return ApiResult.Fail("用户不存在", ResultCode.NotFound);
            }

            // 2. 验证所有传入的角色ID是否存在
            foreach (var roleId in roleIds)
            {
                var roleExists = await _roleRepository.AnyAsync(r => r.Id == roleId);
                if (!roleExists)
                {
                    return ApiResult.Fail($"角色ID {roleId} 不存在", ResultCode.NotFound);
                }
            }

            // 获取用户当前的角色关联 (只包含非删除的)
            var activeUserRoles = (await _userRoleRepository.GetQueryableAsync()).Where(ur => ur.UserId == userId).ToList();
            var activeRoleIds = activeUserRoles.Select(ur => ur.RoleId).ToList();

            // 获取所有（包括软删除）该用户的角色关联
            var queryableAllUserRoles = await _userRoleRepository.GetQueryableAsync();
            var allUserRolesList = await queryableAllUserRoles.AsNoTracking().IgnoreQueryFilters()
                                                                .Where(ur => ur.UserId == userId)
                                                                .ToListAsync();

            // 逻辑步骤2：找出需要软删除的角色ID (当前有，但新列表没有)
            var rolesToSoftDelete = activeRoleIds.Except(roleIds).ToList();

            // 逻辑步骤3：找出需要新增或恢复的角色ID (新列表有，但当前没有)
            var rolesToCreateOrRestore = roleIds.Except(activeRoleIds).ToList();

            // 逻辑步骤4：执行软删除
            foreach (var roleId in rolesToSoftDelete)
            {
                var userRoleToSoftDelete = activeUserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
                if (userRoleToSoftDelete != null)
                {
                    await _userRoleRepository.DeleteAsync(userRoleToSoftDelete); // This performs soft delete
                }
            }

            // 逻辑步骤5：执行新增或恢复
            foreach (var roleId in rolesToCreateOrRestore)
            {
                // 查找是否存在已存在的（包括软删除的）记录
                var existingUserRole = allUserRolesList.FirstOrDefault(ur => ur.RoleId == roleId);

                if (existingUserRole != null)
                {
                    // 如果存在且是软删除状态，则恢复
                    if (existingUserRole.IsDeleted) // Assuming UserRole implements ISoftDelete
                    {
                        existingUserRole.IsDeleted = false;
                        existingUserRole.DeletionTime = null;
                        await _userRoleRepository.UpdateAsync(existingUserRole);
                    }
                    // 如果存在且不是软删除状态，则不做任何操作 (理论上不应该进入这个分支，因为 rolesToCreateOrRestore 已经排除了活跃的)
                }
                else
                {
                    // 不存在任何记录（包括软删除），则新增
                    await _userRoleRepository.InsertAsync(new UserRole(GuidGenerator.Create()) { UserId = userId, RoleId = roleId });
                }
            }

            return ApiResult.Success(ResultCode.Success);
        }
    }
}