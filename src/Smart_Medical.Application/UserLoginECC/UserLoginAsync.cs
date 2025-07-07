using AutoMapper.Internal.Mappers;
using MD5Hash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Smart_Medical.Dictionarys;
using Smart_Medical.Dictionarys.DictionaryTypes;
using Smart_Medical.RBAC;
using Smart_Medical.RBAC.Roles;
using Smart_Medical.RBAC.Users;
using Smart_Medical.Until;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;

namespace Smart_Medical.UserLoginECC
{
    /// <summary>
    /// 用户登录
    /// </summary>
    [ApiExplorerSettings(GroupName = "用户登录")]
    public class UserLoginAsync : ApplicationService, IUserLoginAsyncService
    {

        private readonly IDistributedCache<TokenPairDto> _refreshTokenCache;
        private readonly LMZTokenHelper _tokenHelper;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly IDistributedCache _redisCache;
        private readonly IRepository<Permission, Guid> _permission;
        private readonly IRepository<RBAC.Role, Guid> _roleRepository;
        private readonly IRepository<UserRole, Guid> _userRoleRepository;

        public UserLoginAsync(
                IRepository<User, Guid> userRepository,
                IConfiguration configuration,
                IDistributedCache<TokenPairDto> refreshTokenCache,
                IDistributedCache redisCache, // 新增
                LMZTokenHelper tokenHelper,
                IRepository<Permission, Guid> permission,
                IRepository<RBAC.Role, Guid> roleRepository,
                IRepository<UserRole, Guid> userRoleRepository
                )
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _refreshTokenCache = refreshTokenCache;
            _redisCache = redisCache; // 新增赋值
            _tokenHelper = tokenHelper;
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _permission = permission;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        public async Task<ApiResult<UserDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // 根据用户名查找用户
                var users = await _userRepository.GetQueryableAsync();
                var user = users.FirstOrDefault(u => u.UserName == loginDto.UserName);

                // 检查用户是否存在
                if (user == null)
                {
                    return ApiResult<UserDto>.Fail("用户名不存在", ResultCode.NotFound);
                }

                // 验证密码
                if (user.UserPwd != loginDto.UserPwd.GetMD5())
                {
                    return ApiResult<UserDto>.Fail("密码错误", ResultCode.ValidationError);
                }

                // 登录成功，返回用户信息
                var userDto = ObjectMapper.Map<User, UserDto>(user);
                //userDto.RoleName = null;

                // 获取用户角色
                //var quary = await _userRoleRepository.GetQueryableAsync();
                //var RoleId = quary.Include(x => x.Role).Where(x=>x.UserId== userDto.Id).Select(x=>x.RoleId);

                userDto.RoleName = await _userRoleRepository.GetQueryableAsync()
                                .ContinueWith(t => t.Result //ContinueWith 用于“前一个任务完成后，继续执行下一个任务”，实现任务的链式编排和回调处理。
                                .Where(x => x.UserId == userDto.Id)
                                .Include(x => x.Role)
                                .Select(x => x.Role.RoleName)
                                .ToList());


                //权限
                /*  var permissiondtos = await _permission.GetQueryableAsync();
                  permissiondtos = permissiondtos.Where(x => x.Type == Enums.PermissionType.Button);
                  userDto.Permissions = permissiondtos.Select(x => x.PermissionCode).ToList();*/


                //token获取
                var tokens = await GenerateTokensAsync(user);

                userDto.AccessToken = tokens.AccessToken;
                userDto.RefreshToken = tokens.RefreshToken;
                userDto.AccessTokenExpires = tokens.AccessTokenExpires;
                userDto.RefreshTokenExpires = tokens.RefreshTokenExpires;
                return ApiResult<UserDto>.Success(userDto, ResultCode.Success);
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 生成双token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<TokenPairDtos> GenerateTokensAsync(User user)
        {
            LMZTokenHelper token = new LMZTokenHelper(_configuration);

            // 1. AccessToken
            var accessToken = token.CreateJwtToken(user);
            var accessTokenExpires = DateTime.UtcNow.AddMinutes(10); //10分钟有效

            // 2. RefreshToken
            var refreshToken = token.CreateJwtToken(user, 30);
            var refreshTokenExpires = DateTime.UtcNow.AddDays(7);//7天过期

            var cacheKey = $"SmartMedical:RefreshToken{user.Id}";

            var cacheItem = new TokenPairDto
            {
                RefreshToken = refreshToken,
                RefreshTokenExpires = refreshTokenExpires
            };

            //redis中的过期时间
            var expiresIn = refreshTokenExpires - DateTime.UtcNow;

            await _refreshTokenCache.SetAsync(
                cacheKey,
                cacheItem,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiresIn
                }
            );

            // 4. 返回前端
            return new TokenPairDtos
            {
                AccessToken = accessToken,
                AccessTokenExpires = accessTokenExpires,
                RefreshToken = refreshToken,
                RefreshTokenExpires = refreshTokenExpires
            };
        }

        /// <summary>
        /// 刷新令牌接口
        /// </summary>
        /// <param name="input">包含旧的 RefreshToken</param>
        /// <returns>新的 AccessToken 和 RefreshToken</returns>
        [HttpPost] // 通过 HTTP Post 访问
        [Route("refresh")] // 路由会是 /api/app/authentication/refresh (默认路由前缀)
        [AllowAnonymous] // 刷新令牌接口通常不需要 AccessToken 授权，所以允许匿名访问
        public async Task<ResultLoginDto> RefreshTokenAsync(RefreshTokenRequestDto input)
        {
            if (string.IsNullOrWhiteSpace(input.RefreshToken))
            {
                throw new UserFriendlyException("刷新令牌不能为空", "400");
            }

            Guid userId;
            // 声明 principal 为 ClaimsPrincipal 类型
            ClaimsPrincipal principal;
            try
            {
                principal = _tokenHelper.GetPrincipalFromRefreshToken(input.RefreshToken);

                var userIdClaim = principal?.Claims.FirstOrDefault(c => c.Type == "Id"); // <-- 根据截图，这里用 "Id" 字符串

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out userId))
                {
                    // 如果找不到用户ID的声明，或者解析失败，说明令牌有问题
                    throw new UserFriendlyException("刷新令牌无效或无法解析用户ID", "401");
                }
            }
            // 专门捕获 RefreshToken 过期异常
            catch (SecurityTokenExpiredException)
            {
                throw new UserFriendlyException("刷新令牌已过期，请重新登录", "401");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "解析刷新令牌失败: {RefreshToken}", input.RefreshToken);
                throw new UserFriendlyException("刷新令牌无效或签名验证失败，请重新登录", "401");
            }

            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new UserFriendlyException("用户不存在", "404");
            }

            // 6. 生成新的令牌对 (AccessToken 和新的 RefreshToken)
            var newTokens = await GenerateTokensAsync(user); // GenerateTokensAsync 内部会存储新的 RefreshToken

            // 7. 返回新的令牌信息
            var resultDto = ObjectMapper.Map<User, ResultLoginDto>(user);
            resultDto.AccessToken = newTokens.AccessToken;
            resultDto.RefreshToken = newTokens.RefreshToken;
            resultDto.AccessTokenExpires = newTokens.AccessTokenExpires;
            resultDto.RefreshTokenExpires = newTokens.RefreshTokenExpires;

            return resultDto;
        }

        /// <summary>
        /// 用户登出（将当前 Token 加入黑名单）
        /// </summary>
        /// <param name="input">包含刷新 Token 的请求 DTO</param>
        /// <returns>登出操作的 API 返回结果</returns>
        public async Task<ApiResult> Logout(RefreshTokenRequestDto input)
        {
            try
            {
                //  从请求中获取 AccessToken
                var accessToken = input.RefreshToken;

                // 检查是否为空
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return ApiResult.Fail("未提供有效的 Token", ResultCode.Error);
                }

                //  创建 JWT 解析器，用于解密并读取 Token 内容
                var handler = new JwtSecurityTokenHandler();

                //  判断 token 格式是否合法
                if (!handler.CanReadToken(accessToken))
                {
                    return ApiResult.Fail("无效的 Token", ResultCode.Error);
                }

                // 解读 Token，拿到里面的 Claims 信息（JWT载荷部分）
                var token = handler.ReadJwtToken(accessToken);

                var principal = _tokenHelper.GetPrincipalFromRefreshToken(accessToken);
                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                // 获取 Exp（过期时间戳），转为 UTC 时间
                var expUnix = long.Parse(token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value ?? "0");
                var expTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

                //计算 Token 剩余生存时间（用于设置 Redis 黑名单 TTL）
                var ttl = expTime - DateTime.UtcNow;

                // 若没有 JTI，无法进行黑名单标记，直接失败返回
                if (string.IsNullOrWhiteSpace(jti))
                {
                    return ApiResult.Fail("Token 中缺少 jti", ResultCode.NotFound);
                }

                //构造 Redis 黑名单 Key 和 Value
                var cacheKey = $"blacklist:token:{jti}";     // 黑名单 Key，用 JTI 做标识
                var cacheItem = "blacklisted";

                // 将该 Token JTI 写入 Redis，设置过期时间为 Token 剩余有效时间
                await _redisCache.SetStringAsync(cacheKey, cacheItem, new DistributedCacheEntryOptions
                {
                    //过期时间
                    AbsoluteExpirationRelativeToNow = ttl > TimeSpan.Zero ? ttl : TimeSpan.FromMinutes(1)
                });

                //返回成功结果
                return ApiResult.Success(ResultCode.Success);
            }
            catch (Exception ex)
            {
                // 捕获异常，包一层更具体的上下文信息
                throw new Exception("登出异常", ex);
            }
        }


    }
}
