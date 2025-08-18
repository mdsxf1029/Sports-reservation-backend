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
using Sports_reservation_backend.Utils;
[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly OracleDbContext _context;
    private readonly IConfiguration _config;

    public UploadController(IWebHostEnvironment env, OracleDbContext context, IConfiguration config)
    {
        _env = env;
        _context = context;
        _config = config;
    }

    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequest request)
    {
        var avatar = request.Avatar;

        if (avatar == null || avatar.Length == 0)
        {
            return BadRequest(new
            {
                code = 1001,
                msg = "未选择文件",
                data = (object)null
            });
        }

        // 验证格式
        var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
        var ext = Path.GetExtension(avatar.FileName).ToLower();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(new
            {
                code = 1001,
                msg = "文件格式不支持",
                data = (object)null
            });
        }

        // 保存文件
        var fileName = $"{Guid.NewGuid()}{ext}";
        var savePath = Path.Combine(_env.WebRootPath, "uploads", "avatar");
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        var filePath = Path.Combine(savePath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        var avatarUrl = $"{Request.Scheme}://{Request.Host}/uploads/avatar/{fileName}";

        var authHeader = Request.Headers["Authorization"].ToString();
        Console.WriteLine($"AuthHeader: {authHeader}");
        string token = null;
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
        }
        Console.WriteLine($"Token: {token}");


        if (!string.IsNullOrEmpty(token))
        {
            var principal = JwtTokenUtil.ValidateToken(token, _config);
            if (principal != null)
            {
                var userId = principal.FindFirst("userId")?.Value;
                if (userId != null)
                {
                    var user = await _context.UserSet.FindAsync(int.Parse(userId));
                    if (user != null)
                    {
                        user.AvatarUrl = avatarUrl;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            if (principal == null)
            {
                Console.WriteLine("Token 验证失败，可能在注册时候进行头像的上传。");
            }
        }

        return Ok(new
        {
            code = 0,
            msg = "头像上传成功",
            data = new { avatarUrl }
        });
    }
}
