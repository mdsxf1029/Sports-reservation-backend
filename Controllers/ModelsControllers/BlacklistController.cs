using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFromBlacklist(
            [FromBody] RemoveBlacklistRequest request
        )
        {
            try
            {
                if (request == null || request.UserId <= 0 || request.BeginTime == default)
                    return Ok(
                        new
                        {
                            code = 1,
                            msg = "请求参数不合法",
                            data = (object?)null,
                        }
                    );

                // 1. 查找用户
                var user = await _db.UserSet.FindAsync(request.UserId);
                if (user == null)
                    return Ok(
                        new
                        {
                            code = 1,
                            msg = "用户不存在或不在黑名单中",
                            data = (object?)null,
                        }
                    );

                // 2. 查找对应黑名单记录（根据 userId + beginTime + BannedStatus=valid）
                var blacklist = await _db.BlacklistSet.FirstOrDefaultAsync(b =>
                    b.UserId == request.UserId
                    && b.BeginTime == request.BeginTime
                    && b.BannedStatus == "valid"
                );

                if (blacklist == null)
                    return Ok(
                        new
                        {
                            code = 1,
                            msg = "用户不存在或不在黑名单中",
                            data = (object?)null,
                        }
                    );

                // 3. 移除黑名单（改状态为 invalid 或者删除）
                blacklist.BannedStatus = "invalid";
                blacklist.EndTime = DateTime.UtcNow.AddHours(8);

                await _db.SaveChangesAsync();

                // 4. 返回结果
                return Ok(
                    new
                    {
                        code = 0,
                        msg = "用户已从黑名单移除",
                        data = new { userId = user.UserId, userName = user.UserName },
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除黑名单失败");
                return Ok(
                    new
                    {
                        code = 1,
                        msg = "操作失败，请稍后重试",
                        data = (object?)null,
                    }
                );
            }
        }

        [Authorize]
        [HttpPost("batch-remove")]
        public async Task<IActionResult> BatchRemoveFromBlacklist(
            [FromBody] BatchRemoveBlacklistRequest request
        )
        {
            if (request?.BlacklistItems == null || !request.BlacklistItems.Any())
                return Ok(
                    new
                    {
                        code = 1002,
                        msg = "所有用户移除失败",
                        data = new
                        {
                            removedCount = 0,
                            removedUsers = new object[] { },
                            failedUsers = new object[] { },
                        },
                    }
                );

            var removedUsers = new List<object>();
            var failedUsers = new List<object>();
            var now = DateTime.UtcNow.AddHours(8);

            try
            {
                foreach (var item in request.BlacklistItems)
                {
                    // 查找用户
                    var user = await _db.UserSet.FindAsync(item.UserId);
                    if (user == null)
                    {
                        failedUsers.Add(
                            new
                            {
                                userId = item.UserId,
                                userName = "未知",
                                beginTime = item.BeginTime,
                                reason = "用户不存在",
                            }
                        );
                        continue;
                    }

                    // 查找对应黑名单
                    var blacklist = await _db.BlacklistSet.FirstOrDefaultAsync(b =>
                        b.UserId == item.UserId
                        && b.BeginTime == item.BeginTime
                        && b.BannedStatus == "valid"
                    );

                    if (blacklist == null)
                    {
                        failedUsers.Add(
                            new
                            {
                                userId = user.UserId,
                                userName = user.UserName,
                                beginTime = item.BeginTime,
                                reason = "用户不在黑名单中或时间不匹配",
                            }
                        );
                        continue;
                    }

                    // 移除黑名单
                    blacklist.BannedStatus = "invalid";
                    blacklist.EndTime = now;

                    removedUsers.Add(
                        new
                        {
                            userId = user.UserId,
                            userName = user.UserName,
                            beginTime = item.BeginTime,
                        }
                    );
                }

                // 保存数据库
                await _db.SaveChangesAsync();

                // 返回结果
                if (removedUsers.Count == request.BlacklistItems.Count)
                {
                    return Ok(
                        new
                        {
                            code = 0,
                            msg = "批量移除成功",
                            data = new
                            {
                                removedCount = removedUsers.Count,
                                removedUsers = removedUsers,
                            },
                        }
                    );
                }
                else if (removedUsers.Count > 0)
                {
                    return Ok(
                        new
                        {
                            code = 1001,
                            msg = "部分用户移除失败",
                            data = new
                            {
                                removedCount = removedUsers.Count,
                                removedUsers = removedUsers,
                                failedUsers = failedUsers,
                            },
                        }
                    );
                }
                else
                {
                    return Ok(
                        new
                        {
                            code = 1002,
                            msg = "所有用户移除失败",
                            data = new
                            {
                                removedCount = 0,
                                removedUsers = new object[] { },
                                failedUsers = failedUsers,
                            },
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量移除黑名单失败");
                return Ok(
                    new
                    {
                        code = 1003,
                        msg = "服务器内部错误",
                        data = (object?)null,
                    }
                );
            }
        }
    }
}
