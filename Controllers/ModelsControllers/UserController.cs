using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Sports_reservation_backend.Models.RequestModels;
using System.Security.Claims;

namespace Sports_reservation_backend.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly OracleDbContext _db;
        private readonly ILogger<UserController> _logger;

        public UserController(OracleDbContext db, ILogger<UserController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// 根据 userId 获取用户信息
        /// </summary>
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _db.UserSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return Ok(ApiResponse<object>.Fail(1001, "查询失败"));
                }

                return Ok(ApiResponse<object>.Success(new
                {
                    user.UserName,
                    user.Telephone,
                    user.Email,
                    user.Password,
                    user.Gender,
                    user.Birthday,
                    user.AvatarUrl,
                    user.Region,
                    user.Profile,
                    user.Role,
                    register_time = user.RegisterTime,
                    user.Points
                }, "查询成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询用户信息失败");
                return Ok(ApiResponse<object>.Fail(1001, "查询失败"));
            }
        }

        /// <summary>
        /// 获取当前登录用户信息（从 JWT 获取 userId）
        /// </summary>
        [Authorize]
        [HttpGet("token_to_userId")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdStr = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Ok(ApiResponse<object>.Fail(1001, "Token 无效"));
                }

                var user = await _db.UserSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return Ok(ApiResponse<object>.Fail(1001, "查询失败"));
                }

                return Ok(ApiResponse<object>.Success(new
                {
                    user.UserName,
                    user.Telephone,
                    user.Email,
                    user.Password,
                    user.Gender,
                    user.Birthday,
                    user.AvatarUrl,
                    user.Region,
                    user.Profile,
                    user.Role,
                    register_time = user.RegisterTime,
                    user.Points
                }, "查询成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询当前用户信息失败");
                return Ok(ApiResponse<object>.Fail(1001, "查询失败"));
            }
        }

        [Authorize]
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserRequest request)
        {
            try
            {
                var user = await _db.UserSet.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return Ok(new
                    {
                        code = 1001,
                        msg = "失败",
                        data = (object)null
                    });
                }

                // 如果传了 currentPassword，先校验
                if (!string.IsNullOrEmpty(request.CurrentPassword))
                {
                    if (user.Password != request.CurrentPassword)
                    {
                        return Ok(new
                        {
                            code = 1001,
                            msg = "当前密码错误",
                            data = (object)null
                        });
                    }
                }

                // 如果更新了邮箱且邮箱发生变化，检查是否被其他用户占用
                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    bool emailExists = await _db.UserSet.AnyAsync(u => u.Email == request.Email && u.UserId != userId);
                    if (emailExists)
                    {
                        return Ok(new
                        {
                            code = 1001,
                            msg = "邮箱已被其他用户占用",
                            data = (object)null
                        });
                    }
                }

                // 更新字段，注意要用 null 合并操作符，避免前端没传字段导致覆盖为空
                user.UserName = request.UserName ?? user.UserName;
                user.Telephone = request.Telephone ?? user.Telephone;
                user.Email = request.Email ?? user.Email;
                user.Gender = request.Gender ?? user.Gender;
                user.Birthday = request.Birthday ?? user.Birthday;
                user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
                user.Region = request.Region ?? user.Region;
                user.Profile = request.Profile ?? user.Profile;

                if (!string.IsNullOrEmpty(request.NewPassword))
                {
                    user.Password = request.NewPassword;
                }

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    code = 0,
                    msg = "更新成功",
                    data = new
                    {
                        avatarUrl = user.AvatarUrl,
                        userId = user.UserId.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户信息失败");
                return Ok(new
                {
                    code = 1001,
                    msg = "失败",
                    data = (object)null
                });
            }
        }

    }
}
