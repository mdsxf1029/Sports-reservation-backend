using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/auth")]
[ApiController]
[SwaggerTag("用户注册与登录相关API")]
public class RegisterLoginController : ControllerBase
{
    private readonly OracleDbContext _context;

    public RegisterLoginController(OracleDbContext context)
    {
        _context = context;
    }

    // 注册接口
    [HttpPost]
    [SwaggerOperation(Summary = "用户注册")]
    [SwaggerResponse(201, "注册成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(409, "邮箱已被注册")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (await _context.UserSet.AnyAsync(u => u.Email == model.Email))
        {
            return Conflict($"邮箱 {model.Email} 已被注册");
        }

        var newUser = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            Password = model.Password,
            Telephone = model.Telephone,
            AvatarUrl = model.AvatarUrl ?? string.Empty,
            Gender = model.Gender,
            Birthday = model.Birthday,
            Profile = model.Profile ?? string.Empty,
            Region = model.Region ?? string.Empty,
            RegisterTime = DateTime.UtcNow,
            Points = 0,
            Role = "normal"
        };

        try
        {
            _context.UserSet.Add(newUser);
            await _context.SaveChangesAsync();
            return CreatedAtAction(null, new { email = newUser.Email }, new { message = "注册成功" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器内部错误: {ex.Message}");
        }
    }

    // 登录接口
    [HttpPost("login")]
    [SwaggerOperation(Summary = "用户登录")]
    [SwaggerResponse(200, "登录成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(401, "邮箱或密码错误")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _context.UserSet.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
            if (user == null)
            {
                return Unauthorized("邮箱或密码错误");
            }

            // 登录成功，返回部分用户信息
            return Ok(new
            {
                user.UserId,
                user.UserName,
                user.Email,
                user.Role,
                user.AvatarUrl,
                user.Points
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"服务器内部错误: {ex.Message}");
        }
    }
}
