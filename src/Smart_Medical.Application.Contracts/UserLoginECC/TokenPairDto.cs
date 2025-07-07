using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.UserLoginECC
{
    public class TokenPairDto
    {       
        /// <summary>
        /// 刷新令牌（Refresh Token），用于在 Access Token 过期后换取新 Token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;        

        /// <summary>
        /// Refresh Token 的过期时间
        /// </summary>
        public DateTime RefreshTokenExpires { get; set; } = DateTime.UtcNow;
    }

    public class TokenPairDtos : TokenPairDto
    {
        /// <summary>
        /// 访问令牌（Access Token），用于接口认证的 JWT 字符串
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token 的过期时间
        /// </summary>
        public DateTime AccessTokenExpires { get; set; } = DateTime.UtcNow;
    }

    // 定义刷新请求的 DTO
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; }
    }
}
