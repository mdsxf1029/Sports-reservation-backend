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
                    user.UserId,
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

                // 更新字段，只有在传入值时才更新，避免覆盖为空
                if (!string.IsNullOrEmpty(request.UserName))
                    user.UserName = request.UserName;
                
                if (!string.IsNullOrEmpty(request.Telephone))
                    user.Telephone = request.Telephone;
                
                if (!string.IsNullOrEmpty(request.Email))
                    user.Email = request.Email;
                
                if (!string.IsNullOrEmpty(request.Gender))
                    user.Gender = request.Gender;
                
                if (request.Birthday.HasValue)
                    user.Birthday = request.Birthday.Value;
                
                if (!string.IsNullOrEmpty(request.AvatarUrl))
                    user.AvatarUrl = request.AvatarUrl;
                
                if (!string.IsNullOrEmpty(request.Region))
                    user.Region = request.Region;
                
                if (!string.IsNullOrEmpty(request.Profile))
                    user.Profile = request.Profile;

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
