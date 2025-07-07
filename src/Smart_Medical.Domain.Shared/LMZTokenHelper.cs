using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical
{
    public class LMZTokenHelper
    {
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        //构造函数注入
        public LMZTokenHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }
        /// <summary>
        /// 创建加密JwtToken
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateUser">登录成功的用户信息</param>
        /// <returns></returns>
        public string CreateJwtToken<T>(T generateUser)
        {
            try
            {
                // 1. 签名算法（比如HS256）
                var signingAlgorithm = SecurityAlgorithms.HmacSha256;

                // 2. 把用户信息转换成声明（Claim）列表，放到Token里
                var claimList = CreateClaimList(generateUser);

                // 3. 从配置里拿秘钥字符串，转成字节数组
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);

                // 4. 把字节数组包装成 SymmetricSecurityKey，这个是JWT库用来代表密钥的类型
                var signingKey = new SymmetricSecurityKey(key);

                // 5. 创建签名凭证，告诉JWT“用这个密钥和这个算法签名”
                var signingCredentials = new SigningCredentials(signingKey, signingAlgorithm);


                var beijingTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "China Standard Time");
                // 6. 准备“身份信息”（Claims）准备“签名秘钥和算法”
                var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],//发布者
                        audience: _configuration["Jwt:Audience"],//接收者
                        claims: claimList,//存放的用户信息
                        notBefore: beijingTime,//发布时间
                        //当前为20秒过期，测试
                        expires: beijingTime.AddSeconds(20),//有效期设置
                        signingCredentials: signingCredentials//数字签名
                    ); 

                // 6. 拼接JWT的各个部分
                string tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

                return tokenStr;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建加密JwtToken{ex.Message}");
            }
        }

        /// <summary>
        /// 创建加密JwtToken
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateUser">登录成功的用户信息</param>
        /// <param name="expiresMinutes">过期时间，默认60分钟</param>
        /// <returns></returns>
        public string CreateJwtToken<T>(T generateUser, int expiresMinutes = 60)
        {
            try
            {
                // 1. 签名算法（比如HS256）
                var signingAlgorithm = SecurityAlgorithms.HmacSha256;

                // 2. 把用户信息转换成声明（Claim）列表，放到Token里
                var claimList = CreateClaimList(generateUser);

                // 3. 从配置里拿秘钥字符串，转成字节数组
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);

                // 4. 把字节数组包装成 SymmetricSecurityKey，这个是JWT库用来代表密钥的类型
                var signingKey = new SymmetricSecurityKey(key);

                // 5. 创建签名凭证，告诉JWT“用这个密钥和这个算法签名”
                var signingCredentials = new SigningCredentials(signingKey, signingAlgorithm);

                var beijingTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "China Standard Time");
                // 6. 准备“身份信息”（Claims）准备“签名秘钥和算法”
                var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],//发布者
                        audience: _configuration["Jwt:Audience"],//接收者
                        claims: claimList,//存放的用户信息
                        notBefore: beijingTime,//发布时间
                        expires: beijingTime.AddMinutes(expiresMinutes),//有效期设置
                        signingCredentials: signingCredentials//数字签名
                    );

                // 6. 拼接JWT的各个部分
                string tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

                return tokenStr;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建加密JwtToken{ex.Message}");
            }
        }

        /// <summary>
        /// 创建包含用户信息的Claim列表
        /// 把对象所有属性转换成一堆键值对（声明），写进 JWT 里，方便验证和识别用户信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="authUser"></param>
        /// <returns></returns>
        public List<Claim> CreateClaimList<T>(T authUser)
        {
            try
            {
                //显示转换举例
                //return new List<Claim>
                //    {
                //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                //        new Claim(ClaimTypes.Name, user.UserName),
                //        new Claim("RealName", user.RealName ?? "")
                //    };

                // 获取类型信息
                var classType = typeof(T);
                var claims = classType.GetProperties()
                    .Select(p => new Claim(
                        p.Name,
                        p.GetValue(authUser)?.ToString() ?? string.Empty
                    )).ToList();

                // 添加一个随机的 Claim（保证 token 每次都不同）
                claims.Add(new Claim("LoginSessionId", Guid.NewGuid().ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                // 加入签发时间
                claims.Add(new Claim("IssuedAt", DateTime.UtcNow.ToString("o"))); // ISO 8601 格式

                return claims;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建包含用户信息的Claim列表{ex.Message}");
            }
        }
        /// <summary>
        /// 从 JWT Token 中提取出所有 Claim，并尝试转换成指定类型 T 的对象
        /// </summary>
        /// <typeparam name="T">目标类型，必须是一个无参构造类</typeparam>
        /// <param name="token">JWT 字符串</param>
        /// <returns>还原出来的 T 类型对象</returns>
        public T ConvertTokenToObj<T>(string token) where T : new()
        {
            var objA = new T();
            var jwtToken = _jwtSecurityTokenHandler.ReadJwtToken(token);

            foreach (var claim in jwtToken.Claims)
            {
                var property = typeof(T).GetProperties()
                    .FirstOrDefault(p => string.Equals(p.Name, claim.Type, StringComparison.OrdinalIgnoreCase));

                if (property == null || !property.CanWrite) continue;
                var targetType = property.PropertyType;

                var convertedvalue = Convert.ChangeType(claim.Value, targetType);
                property.SetValue(objA, convertedvalue);

            }

            return objA;
        }

        /// <summary>
        /// 生成一个安全的 Refresh Token（刷新令牌）
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public string GenerateRefreshToken(int size = 32)
        {
            // 1. 创建一个指定大小的字节数组
            var randomNumber = new byte[size];

            // 2. 使用加密安全的随机数生成器
            using (var rng = RandomNumberGenerator.Create())
            {
                // 3. 生成随机字节
                rng.GetBytes(randomNumber);

                // 4. 将随机字节转换为 Base64 字符串
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// 从刷新令牌中提取用户声明信息（ClaimsPrincipal）
        /// 不会验证 Token 是否过期，只验证签名和 Issuer、Audience 等
        /// </summary>
        /// <param name="token">客户端传来的 JWT Refresh Token</param>
        /// <returns>解析出来的 ClaimsPrincipal（包含用户身份声明）</returns>
        /// <exception cref="SecurityTokenException">当 Token 签名无效或格式错误时抛出异常</exception>
        public ClaimsPrincipal GetPrincipalFromRefreshToken(string token)
        {
            try
            {
                // 创建一个 Token 验证参数对象
                var tokenValidationParameters = new TokenValidationParameters
                {
                    // 验证 Audience（受众，即这个 token 是发给谁的）
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],

                    // 验证 Issuer（发行者，即这个 token 是谁签发的）
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],

                    // 验证签名（确保 token 没被篡改）
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"])),

                    // 不验证过期时间（因为 RefreshToken 可能要手动控制）
                    ValidateLifetime = false
                };

                SecurityToken securityToken;

                // 使用 JwtSecurityTokenHandler 验证并解析 token
                var principal = _jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

                // 确保解析出来的是合法的 JwtSecurityToken 且算法为 HmacSha256（防止算法攻击）
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("无效的刷新令牌或签名不匹配。");
                }

                // 返回解析出来的用户身份信息（可用于获取 Claims，例如 sub、jti、role 等）
                return principal;
            }
            catch (Exception)
            {
                // 捕获异常直接抛出（可以根据需要改成日志记录等）
                throw;
            }
        }

    }
}
