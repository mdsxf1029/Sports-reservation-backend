using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sports_reservation_backend.Utils
{
    public static class JwtTokenUtil
    {
        /// <summary>
        /// 验证 JWT Token，并返回解析后的 ClaimsPrincipal（可用于获取用户 ID 等信息）
        /// </summary>
        /// <param name="token">前端传来的 token 字符串</param>
        /// <param name="config">配置，用于获取密钥</param>
        /// <returns>验证成功则返回 ClaimsPrincipal，失败返回 null</returns>
        public static ClaimsPrincipal? ValidateToken(string token, IConfiguration config)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]); // 从配置中获取密钥

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, // 可选：不验证 Issuer
                ValidateAudience = false, // 可选：不验证 Audience
                ValidateLifetime = true, // 验证过期时间
                ValidateIssuerSigningKey = true, // 验证签名密钥
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero // 不允许时间偏移（默认允许 5 分钟）
            };

            try
            {
                // 验证 token（会抛出异常则说明验证失败）
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null; // 验证失败返回 null
            }
        }
    }
}
