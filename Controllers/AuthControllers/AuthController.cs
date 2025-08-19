using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Models.TableModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sports_reservation_backend.Controllers.AuthControllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly OracleDbContext _db;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;

        public AuthController(OracleDbContext db, ILogger<AuthController> logger, IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (await _db.UserSet.AnyAsync(u => u.Telephone == request.Telephone))
                    return Ok(ApiResponse<object>.Fail(1001, "手机号已被注册"));

                if (await _db.UserSet.AnyAsync(u => u.Email == request.Email))
                    return Ok(ApiResponse<object>.Fail(1002, "邮箱已经被注册"));

                var user = new User
                {
                    UserName = request.UserName,
                    Telephone = request.Telephone,
                    Email = request.Email,
                    Password = request.Password,
                    Gender = request.Gender,
                    Birthday = request.Birthday,
                    AvatarUrl = request.AvatarUrl,
                    Region = request.Region,
                    Profile = request.Profile,
                    Role = request.Role
                };

                _db.UserSet.Add(user);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success(new
                {
                    userId = user.UserId,
                    userName = user.UserName
                }, "注册成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册失败");
                return Ok(ApiResponse<object>.Fail(1003, "服务器内部错误"));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _db.UserSet.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                    return Ok(ApiResponse<object>.Fail(1003, "邮箱未注册"));

                if (user.Password != request.Password)
                    return Ok(ApiResponse<object>.Fail(1002, "账号密码错误"));

                var jwtKey = _config["Jwt:SecretKey"]!;
                var jwtIssuer = _config["Jwt:Issuer"]!;
                var jwtAudience = _config["Jwt:Audience"]!;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                var claims = new[]
                {
                    new Claim("userId", user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = jwtIssuer,
                    Audience = jwtAudience,
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                var expires = ((DateTimeOffset)tokenDescriptor.Expires!).ToUnixTimeSeconds();

                return Ok(ApiResponse<object>.Success(new
                {
                    userId = user.UserId,
                    userName = user.UserName,
                    token = tokenString,
                    expires = expires
                }, "账号登陆成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败");
                return Ok(ApiResponse<object>.Fail(1001, "服务器内部错误"));
            }
        }
    }
}