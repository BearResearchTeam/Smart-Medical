using MD5Hash;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Smart_Medical.RBAC; // 引入Domain层的RBAC实体
using Smart_Medical.Until.Redis;
using Smart_Medical.RBAC.Permissions;
using Smart_Medical.Patient;
using Volo.Abp.Uow;
using Smart_Medical.Application.Contracts.RBAC.Users;

namespace Smart_Medical.RBAC.Users
{
    [ApiExplorerSettings(GroupName = "用户管理")]
    public class UserAppService : ApplicationService, IUserAppService
    {
        // 定义一个常量作为缓存键，这是这个特定缓存项在 Redis 中的唯一标识。
        // 使用一个清晰且唯一的键很重要。
        private const string CacheKey = "permission:All"; // 建议使用更具体的键名和前缀
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<UserPatient, Guid> _userPatientRepo;
        private readonly IRepository<UserRole, Guid> _userRoleRepo;
        private readonly IRepository<BasicPatientInfo, Guid> _patientRepo;
           private readonly IRedisHelper<List<PermissionDto>> redisHelper;
        private readonly IRepository<Permission, Guid> permission;

        public UserAppService(
            IRepository<User, Guid> userRepository,
            IRepository<UserPatient, Guid> userPatientRepo,
            IRepository<UserRole, Guid> userRoleRepo,
            IRepository<BasicPatientInfo, Guid> patientRepo,IRedisHelper<List<PermissionDto>> redisHelper,IRepository<Permission,Guid> permission
            )
        {
            _userRepository = userRepository;
            _userPatientRepo = userPatientRepo;
            _userRoleRepo = userRoleRepo;
            _patientRepo = patientRepo;
            this.redisHelper = redisHelper;
            this.permission = permission;
        }
      

        /// <summary>
        /// 用户注册（基础功能，手动分配一个默认角色）
        /// </summary>
        /// <param name="input">用户创建信息</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> InsertUserPTAsync(CreateUpdateUserDto input)
        {
            input.UserPwd = input.UserPwd.GetMD5();
            var user = ObjectMapper.Map<CreateUpdateUserDto, User>(input);
            var result = await _userRepository.InsertAsync(user);
            Guid roleid = Guid.Parse("7741f103-2101-2771-d651-3a1ab712ee78"); // 角色：用户
            // 直接创建用户角色关联
            var userRole = new UserRole(GuidGenerator.Create())
            {
                UserId = result.Id,
                RoleId = roleid
            };
            // BUG修复：之前创建的用户角色关联对象没有被保存到数据库
            await _userRoleRepo.InsertAsync(userRole);
            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 按ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                return ApiResult.Fail("用户不存在", ResultCode.NotFound);
            }
            await _userRepository.DeleteAsync(user);
            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 按ID查询单个用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>包含用户详细信息的ApiResult</returns>
        public async Task<ApiResult<UserDto>> GetAsync(Guid id)
        {
            var queryable = await _userRepository.GetQueryableAsync();

            // 为了能在后续投影中安全地获取角色名，需要Include关联的角色数据
            queryable = queryable.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);

            // 直接使用投影查询将结果映射到UserDto，避免加载完整实体和使用AutoMapper
            var userDto = await AsyncExecuter.FirstOrDefaultAsync(
                queryable
                    .Where(u => u.Id == id)
                    .Select(u => new UserDto
                    {
                        UserName = u.UserName,
                        UserEmail = u.UserEmail,
                        UserPhone = u.UserPhone,
                        UserSex = u.UserSex,
                        // EF Core表达式树不支持空传播运算符(?.), 改用Select().FirstOrDefault()达到同样的安全效果
                        RoleName = u.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault()
                    })
            );


            if (userDto == null)
            {
                return ApiResult<UserDto>.Fail("用户不存在", ResultCode.NotFound);
            }
            return ApiResult<UserDto>.Success(userDto, ResultCode.Success);
        }
        /// <summary>
        /// 根据查询条件分页获取用户列表
        /// </summary>
        /// <param name="input">包含分页和筛选信息的查询DTO</param>
        /// <returns>包含用户列表和分页信息的ApiResult</returns>
        public async Task<ApiResult<PageResult<List<UserDto>>>> GetListAsync([FromQuery] SeachUserDto input)
        {
            var queryable = await _userRepository.GetQueryableAsync();

            // BUG修复：为了能在后续投影中安全地获取角色名，需要Include关联的角色数据
            queryable = queryable
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
                //.Include(x=>x.UserPatients).ThenInclude(up => up.Patient);

            if (!string.IsNullOrWhiteSpace(input.UserName))
            {
                queryable = queryable.Where(u => u.UserName.Contains(input.UserName));
            }
            if (!string.IsNullOrWhiteSpace(input.UserEmail))
            {
                queryable = queryable.Where(u => u.UserEmail.Contains(input.UserEmail));
            }
            if (!string.IsNullOrWhiteSpace(input.UserPhone))
            {
                queryable = queryable.Where(u => u.UserPhone.Contains(input.UserPhone));
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .OrderBy(u => u.UserName); // 默认排序

            // 使用 Select 进行投影，直接生成 UserDto 列表，绕过 AutoMapper
            var userDtos = await AsyncExecuter.ToListAsync(
                queryable.Select(u => new UserDto
                {
                    UserName = u.UserName,
                    UserEmail = u.UserEmail,
                    UserPhone = u.UserPhone,
                    UserSex = u.UserSex,
                    // EF Core表达式树不支持空传播运算符(?.), 改用Select().FirstOrDefault()达到同样的安全效果
                    RoleName = u.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault()

                    // 根据你的要求，不再映射审计字段
                })
            );

            var pageResult = new PageResult<List<UserDto>>
            {
                TotleCount = totalCount,
                TotlePage = (int)Math.Ceiling((double)totalCount / input.MaxResultCount),
                Data = userDtos
            };

            return ApiResult<PageResult<List<UserDto>>>.Success(pageResult, ResultCode.Success);
        }

        /// <summary>
        /// 根据用户名获取用户实体
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>包含用户信息的ApiResult</returns>
        public async Task<ApiResult<UserDto>> GetByUserNameAsync(string username)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return ApiResult<UserDto>.Fail("用户不存在", ResultCode.NotFound);
            }
            var userDto = ObjectMapper.Map<User, UserDto>(user);
            return ApiResult<UserDto>.Success(userDto, ResultCode.Success);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="loginDto">登录信息DTO</param>
        /// <returns>成功则返回用户信息，否则返回错误信息</returns>
        public async Task<ApiResult<UserDto>> LoginAsync(LoginDto loginDto)
        {
            // 1. 根据用户名查找用户
            var users = await _userRepository.GetQueryableAsync();
            //// 联查用户-角色-权限
            //var user = await users
            //    .Include(u => u.UserRoles)
            //        .ThenInclude(ur => ur.Role)
            //            .ThenInclude(r => r.RolePermissions)
            //                .ThenInclude(rp => rp.Permission)
            //    .FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);
            var user= await users.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);

            // 2. 检查用户是否存在
            if (user == null)
            {
                return ApiResult<UserDto>.Fail("用户名不存在", ResultCode.NotFound);
            }

            // 3. 验证密码
            if (user.UserPwd != loginDto.UserPwd.GetMD5())
            {
                return ApiResult<UserDto>.Fail("密码错误", ResultCode.ValidationError);
            }

            // 4. 登录成功，组装用户信息
            var userDto = ObjectMapper.Map<User, UserDto>(user);

            // 5. 角色列表
            //userDto.Roles = user.UserRoles?
            //    .Select(ur => ur.Role?.RoleName)
            //    .Where(rn => !string.IsNullOrEmpty(rn))
            //    .Distinct()
            //    .ToList() ?? new List<string>();

            //// 6. 权限列表（去重，返回权限编码或名称均可）
            //userDto.Permissions = user.UserRoles?
            //    .SelectMany(ur => ur.Role?.RolePermissions ?? new List<RolePermission>())
            //    .Select(rp => rp.Permission?.PermissionCode)
            //    .Where(pc => !string.IsNullOrEmpty(pc))
            //    .Distinct()
            //    .ToList() ?? new List<string>();
            var permissiondtos= await permission.GetQueryableAsync();
            permissiondtos= permissiondtos.Where(x=>x.Type==Enums.PermissionType.Button);
            userDto.Permissions = permissiondtos.Select(x=>x.PermissionCode).ToList();

            return ApiResult<UserDto>.Success(userDto, ResultCode.Success);
        }

        /// <summary>
        /// 根据ID更新用户信息
        /// </summary>
        /// <param name="id">要更新的用户ID</param>
        /// <param name="input">包含新用户信息的DTO</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> UpdateAsync(Guid id, CreateUpdateUserDto input)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                return ApiResult.Fail("用户不存在", ResultCode.NotFound);
            }

            ObjectMapper.Map(input, user);
            user.UserPwd = input.UserPwd.GetMD5(); // 确保密码被加密
            await _userRepository.UpdateAsync(user);
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 用户注册（自动分配角色并建立患者关联）
        /// </summary>
        [HttpPost]
        [UnitOfWork]//事务
        public async Task<ApiResult> BasRegisterAsync(RegisterUserDto input)
        {
            try
            {

                // 1. 输入验证
                if (input == null)
                {
                    return ApiResult.Fail("注册信息不能为空", ResultCode.ValidationError);
                }

                // 2. 检查用户名是否已存在
                var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.UserName == input.UserName);
                if (existingUser != null)
                {
                    return ApiResult.Fail("用户名已存在", ResultCode.ValidationError);
                }

                // 3. 检查邮箱是否已存在
                existingUser = await _userRepository.FirstOrDefaultAsync(u => u.UserEmail == input.UserEmail);
                if (existingUser != null)
                {
                    return ApiResult.Fail("邮箱已被使用", ResultCode.ValidationError);
                }

                // 4. 创建用户
                var user = new User
                {
                    UserName = input.UserName,
                    UserPwd = input.UserPwd.GetMD5(),
                    UserEmail = input.UserEmail,
                    UserPhone = input.UserPhone,
                    UserSex = input.UserSex
                };

                var userinfo = await _userRepository.InsertAsync(user);

                // 5. 分配角色
                var roleId = Guid.Parse("7741f103-2101-2771-d651-3a1ab712ee78"); //固定是患者角色

                var userRole = new UserRole(GuidGenerator.Create())
                {
                    UserId = userinfo.Id,
                    RoleId = roleId
                };
                await _userRoleRepo.InsertAsync(userRole);

                // 6. 创建患者信息
                var patient = new BasicPatientInfo
                {
                    PatientName = input.UserName,
                    Age = input.Age,
                    AgeUnit = input.AgeUnit ?? "年",
                    Gender = input.UserSex.HasValue ? (input.UserSex.Value ? 1 : 2) : 1,
                    ContactPhone = input.UserPhone,
                    IdNumber = input.IdNumber ?? "",
                    VisitType = input.VisitType ?? "初诊",
                    IsInfectiousDisease = input.IsInfectiousDisease,
                    DiseaseOnsetTime = input.DiseaseOnsetTime,
                    EmergencyTime = input.EmergencyTime,
                    VisitStatus = input.VisitStatus ?? "待就诊",
                    VisitDate = input.VisitDate
                };

                var patientinfo = await _patientRepo.InsertAsync(patient);

                // 7. 建立用户-患者关联
                var userPatient = new UserPatient(GuidGenerator.Create())
                {
                    UserId = userinfo.Id,
                    PatientId = patientinfo.Id
                };
                await _userPatientRepo.InsertAsync(userPatient);

                return ApiResult.Success(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"注册失败: {ex.Message}", ResultCode.Error);
            }
        }

        /// <summary>
        ///根据用户添加并关联患者信息
        /// </summary>
        [HttpPost]
        [UnitOfWork]
        public async Task<ApiResult> AddPatientInfoAsync(Guid userId, AddPatientInfoDto input)
        {
            try
            {
                // 1. 验证用户是否存在
                var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return ApiResult.Fail("用户不存在", ResultCode.NotFound);
                }

                // 2. 检查患者身份证号是否已存在
                if (!string.IsNullOrWhiteSpace(input.IdNumber))
                {
                    var existingPatient = await _patientRepo.FirstOrDefaultAsync(p => p.IdNumber == input.IdNumber);
                    if (existingPatient != null)
                    {
                        return ApiResult.Fail("该身份证号已被注册", ResultCode.ValidationError);
                    }
                }

                // 3. 创建患者信息
                var patient = ObjectMapper.Map<AddPatientInfoDto, BasicPatientInfo>(input);
                patient.PatientName = patient.PatientName; // 确保姓名被赋值
                patient.Gender = input.Gender.HasValue ? (input.Gender.Value ? 1 : 2) : 1; // 转换为 int 类型
                patient.AgeUnit = input.AgeUnit ?? "年";
                patient.VisitType = input.VisitType ?? "初诊";
                patient.VisitStatus = input.VisitStatus ?? "待就诊";

                var patientinfo = await _patientRepo.InsertAsync(patient);

                // 4. 建立用户-患者关联
                var userPatient = new UserPatient(GuidGenerator.Create())
                {
                    UserId = userId,
                    PatientId = patientinfo.Id
                };
                await _userPatientRepo.InsertAsync(userPatient);

                return ApiResult.Success(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"添加患者信息失败: {ex.Message}", ResultCode.Error);
            }
        }

        /// <summary>
        /// 【仅为示例】展示如何只查询用户表而不加载关联数据
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<PageResult<List<UserDto>>>> GetSimpleUserListExampleAsync([FromQuery] SeachUserDto input)
        {
            /*
             * 解释：
             * 在很多场景下，我们确实只需要用户表本身的基础数据，而不需要加载它关联的角色、患者等信息。
             * 这么做可以大大提升查询效率，减少数据库和网络开销。
             * 
             * 关键在于，查询出来的实体后续如何被使用。
             * 在之前的 GetListAsync 方法中，我们把包含了所有字段的 User 实体加载到内存，然后让 AutoMapper 把它转换成 UserDto。
             * 因为 UserDto 中有关联属性（角色、患者），所以 AutoMapper 会尝试访问 User 实体上的关联导航属性（UserRoles, UserPatients），
             * 如果我们没有用 .Include() 提前加载这些导航属性，它们就是 null，从而导致映射失败。
             * 
             * 下面的代码展示了如何解决这个问题：使用 "投影" (Projection)。
             * 我们不把整个 User 实体加载到内存，而是通过 .Select() 方法，在数据库查询的环节，就直接将数据构造成我们最终需要的 UserDto 对象。
             * 这样做有两大好处：
             * 1. 高效：生成的SQL语句只会查询我们真正需要的字段（如 UserName, UserEmail)，而不会查询User表中的所有字段，更不会JOIN关联表。
             * 2. 可控：我们精确地控制了哪些数据被填充到 DTO 中，对于不需要的关联属性（如Roles, Patients），我们直接赋一个空列表，这样后续代码（如JSON序列化）就不会出错。
             * 
             * 注意：这种方式下，我们绕过了 AutoMapper，进行的是手动映射。
             */

            var queryable = await _userRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.UserName))
            {
                queryable = queryable.Where(u => u.UserName.Contains(input.UserName));
            }
            if (!string.IsNullOrWhiteSpace(input.UserEmail))
            {
                queryable = queryable.Where(u => u.UserEmail.Contains(input.UserEmail));
            }
            if (!string.IsNullOrWhiteSpace(input.UserPhone))
            {
                queryable = queryable.Where(u => u.UserPhone.Contains(input.UserPhone));
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // 使用 Select 进行投影，直接生成 UserDto 列表
            var userDtos = await AsyncExecuter.ToListAsync(
                queryable
                    .OrderBy(u => u.UserName)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .Select(u => new UserDto
                    {
                        UserName = u.UserName,
                        UserEmail = u.UserEmail,
                        UserPhone = u.UserPhone,
                        UserSex = u.UserSex,
                        // EF Core表达式树不支持空传播运算符(?.), 改用Select().FirstOrDefault()达到同样的安全效果
                        RoleName = u.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault()

                        // 根据你的要求，不再映射审计字段
                    })
            );

            var pageResult = new PageResult<List<UserDto>>
            {
                TotleCount = totalCount,
                TotlePage = (int)Math.Ceiling((double)totalCount / input.MaxResultCount),
                Data = userDtos
            };

            return ApiResult<PageResult<List<UserDto>>>.Success(pageResult, ResultCode.Success);
        }

    }
}
