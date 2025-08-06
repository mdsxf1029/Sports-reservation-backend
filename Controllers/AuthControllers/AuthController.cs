using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers.AuthControllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly OracleDbContext _db;
    private readonly ILogger<AuthController> _logger;

    public AuthController(OracleDbContext db, ILogger<AuthController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // 电话重复校验
            if (await _db.UserSet.AnyAsync(u => u.Telephone == request.Telephone))
                return Ok(ApiResponse<object>.Fail(1001, "手机号已被注册"));

            // 邮箱重复校验
            if (await _db.UserSet.AnyAsync(u => u.Email == request.Email))
                return Ok(ApiResponse<object>.Fail(1002, "邮箱已经被注册"));

            // 创建用户（明文存密码，仅演示）
            var user = new User
            {
                UserName = request.UserName,
                Telephone = request.Telephone,
                Email = request.Email,
                Password = request.Password,  // 明文存储，演示用
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
}

