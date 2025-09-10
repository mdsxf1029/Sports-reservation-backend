using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Models.TableModels;
using Sports_reservation_backend.Utils;

namespace Sports_reservation_backend.Controllers
{
    [ApiController]
    [Route("api/blacklist")]
    public class BlacklistController : ControllerBase
    {
        private readonly OracleDbContext _db;
        private readonly ILogger<BlacklistController> _logger;
        private readonly IConfiguration _config;

        public BlacklistController(
            OracleDbContext db,
            ILogger<BlacklistController> logger,
            IConfiguration config
        )
        {
            _db = db;
            _logger = logger;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetBlacklist()
        {
            try
            {
                var blacklist = await _db
                    .BlacklistSet.Select(b => new
                    {
                        userId = b.UserId,
                        managerId = b.ManagerId,
                        beginTime = b.BeginTime,
                        endTime = b.EndTime,
                        bannedReason = b.BannedReason,
                        bannedStatus = b.BannedStatus,
                    })
                    .ToListAsync();

                return Ok(
                    new
                    {
                        success = true,
                        data = blacklist,
                        message = "获取黑名单成功",
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取黑名单失败");
                return Ok(
                    new
                    {
                        success = false,
                        data = new object[] { },
                        message = "获取黑名单失败",
                    }
                );
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToBlacklist([FromBody] AddBlacklistRequest request)
        {
            try
            {
                if (request.Id <= 0)
                    return BadRequest(new { success = false, message = "用户ID不能为空" });

                // 1. 获取token中的manager_id
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return Unauthorized(new { success = false, message = "未授权" });

                string token = authHeader.Substring("Bearer ".Length).Trim();
                var principal = JwtTokenUtil.ValidateToken(token, _config);
                if (principal == null)
                    return Unauthorized(new { success = false, message = "Token无效" });

                var managerIdStr = principal.FindFirst("userId")?.Value;
                if (managerIdStr == null)
                    return Unauthorized(new { success = false, message = "无法获取管理员ID" });
                int managerId = int.Parse(managerIdStr);

                // 2. 检查用户是否存在
                var user = await _db.UserSet.FindAsync(request.Id);
                if (user == null)
                    return BadRequest(new { success = false, message = "用户不存在" });

                // 3. 添加黑名单记录
                var now = DateTime.UtcNow.AddHours(8);
                ;
                var blacklist = new Blacklist
                {
                    UserId = request.Id,
                    ManagerId = managerId,
                    BeginTime = now,
                    EndTime = request.EndTime,
                    BannedReason = request.BannedReason,
                    BannedStatus = "valid",
                };

                _db.BlacklistSet.Add(blacklist);
                await _db.SaveChangesAsync();

                // 4. 构建返回值，只返回新增的黑名单
                var result = new
                {
                    userId = blacklist.UserId,
                    managerId = blacklist.ManagerId,
                    beginTime = blacklist.BeginTime,
                    endTime = blacklist.EndTime,
                    bannedReason = blacklist.BannedReason,
                    bannedStatus = blacklist.BannedStatus,
                };

                return Ok(
                    new
                    {
                        success = true,
                        data = result,
                        message = "添加用户到黑名单成功",
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加用户到黑名单失败");
                return Ok(
                    new
                    {
                        success = false,
                        data = (object?)null,
                        message = "添加用户到黑名单失败",
                    }
                );
            }
        }
    }
}
