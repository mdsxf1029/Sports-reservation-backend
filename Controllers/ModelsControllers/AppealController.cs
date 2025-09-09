using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers;

[Route("api/appeals")]
[ApiController]
public class AppealController : ControllerBase
{
    private readonly OracleDbContext _db;
    private readonly ILogger<AppealController> _logger;

    public AppealController(OracleDbContext db, ILogger<AppealController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // 获取申诉列表
    [HttpGet]
    public async Task<IActionResult> GetAppeals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? venue = null,
        [FromQuery] int? processor = null,
        [FromQuery] string? keyword = null
    )
    {
        try
        {
            // 1. 基础查询 (多表联查)
            var query =
                from a in _db.AppealSet
                join v in _db.ViolationSet on a.ViolationId equals v.ViolationId
                join u in _db.UserSet on a.UserId equals u.UserId
                join va in _db.VenueAppointmentSet on v.AppointmentId equals va.AppointmentId
                join ve in _db.VenueSet on va.VenueId equals ve.VenueId
                join ts in _db.TimeslotSet on va.TimeslotId equals ts.TimeslotId
                join pu in _db.UserSet on a.ProcessorId equals pu.UserId into processorJoin
                from processorUser in processorJoin.DefaultIfEmpty()
                select new
                {
                    Id = a.AppealId,
                    UserName = u.UserName,
                    UserId = u.UserId,
                    UserAvatar = u.AvatarUrl,
                    ViolationTime = v.ViolationTime,
                    Venue = ve.VenueName,
                    TimeSlot = ts.BeginTime + "-" + ts.EndTime,
                    AppealReason = a.AppealReason,
                    AppealTime = a.AppealTime,
                    AppealStatus = a.AppealStatus,
                    Processor = a.ProcessorId,
                    ProcessorName = processorUser != null ? processorUser.UserName : null,
                    ProcessTime = a.ProcessTime,
                };

            // 2. 过滤条件
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.AppealStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(venue))
            {
                query = query.Where(a => a.Venue.Contains(venue));
            }

            if (processor.HasValue)
            {
                query = query.Where(a => a.Processor == processor.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(a =>
                    a.UserName.Contains(keyword) || a.AppealReason.Contains(keyword)
                );
            }

            // 3. 分页
            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(a => a.AppealTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. 返回
            return Ok(
                new
                {
                    code = 200,
                    total,
                    data,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取申诉列表失败");
            return Ok(new { code = 500, message = "获取失败，请稍后重试" });
        }
    }

    // 根据id查找申诉详情
    [HttpGet("{appealId}")]
    public async Task<IActionResult> GetAppealById(int appealId)
    {
        try
        {
            var appeal = await (
                from a in _db.AppealSet
                join v in _db.ViolationSet on a.ViolationId equals v.ViolationId
                join u in _db.UserSet on a.UserId equals u.UserId
                join va in _db.VenueAppointmentSet on v.AppointmentId equals va.AppointmentId
                join ve in _db.VenueSet on va.VenueId equals ve.VenueId
                join ts in _db.TimeslotSet on va.TimeslotId equals ts.TimeslotId
                join pu in _db.UserSet on a.ProcessorId equals pu.UserId into processorJoin
                from processorUser in processorJoin.DefaultIfEmpty()
                where a.AppealId == appealId
                select new
                {
                    Id = a.AppealId,
                    UserName = u.UserName,
                    UserId = u.UserId,
                    UserAvatar = u.AvatarUrl,
                    ViolationTime = v.ViolationTime,
                    Venue = ve.VenueName,
                    TimeSlot = ts.BeginTime + "-" + ts.EndTime,
                    AppealReason = a.AppealReason,
                    AppealTime = a.AppealTime,
                    AppealStatus = a.AppealStatus,
                    Processor = a.ProcessorId,
                    ProcessorName = processorUser != null ? processorUser.UserName : null,
                    ProcessTime = a.ProcessTime,
                }
            ).FirstOrDefaultAsync();

            if (appeal == null)
            {
                return Ok(
                    new
                    {
                        code = 404,
                        message = "申诉不存在",
                        data = (object?)null,
                    }
                );
            }

            return Ok(new { code = 200, data = appeal });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取申诉详情失败，appealId: {AppealId}", appealId);
            return Ok(new { code = 500, message = "获取失败，请稍后重试" });
        }
    }

    // 添加申诉
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SubmitAppeal([FromBody] SubmitAppealRequest request)
    {
        try
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Ok(new { code = 401, message = "Token 无效" });
            }

            // 1. 验证违约记录是否存在
            var violation = await _db.ViolationSet.FindAsync(request.ViolationId);
            if (violation == null)
            {
                return Ok(new { code = 404, message = "违约记录不存在" });
            }

            // 2. 创建申诉记录
            var appeal = new Appeal
            {
                ViolationId = request.ViolationId,
                UserId = userId,
                AppealReason = request.AppealReason,
                EvidenceUrl =
                    request.EvidenceUrls != null ? string.Join(",", request.EvidenceUrls) : null,
                AppealStatus = "pending",
            };

            _db.AppealSet.Add(appeal);
            await _db.SaveChangesAsync();

            return Ok(new { code = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "提交申诉失败");
            return Ok(new { code = 500, message = "提交失败，请稍后重试" });
        }
    }

    // 处理申诉
    [Authorize]
    [HttpPut("{appealId}/process")]
    public async Task<IActionResult> ProcessAppeal(
        int appealId,
        [FromBody] ProcessAppealRequest request
    )
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Action))
                return Ok(new { code = 400, message = "请求体为空或 Action 缺失" });

            // 1. 获取当前管理员 ID
            var adminIdStr = User.FindFirst("userId")?.Value;
            if (!int.TryParse(adminIdStr, out int adminId))
                return Ok(new { code = 401, message = "Token 无效或未找到管理员 ID" });

            // 2. 查询申诉
            var appeal = await _db
                .AppealSet.Include(a => a.User)
                .Include(a => a.Violation)
                .FirstOrDefaultAsync(a => a.AppealId == appealId);

            if (appeal == null)
                return Ok(new { code = 404, message = "申诉不存在" });

            if (appeal.AppealStatus != "pending")
                return Ok(new { code = 400, message = "该申诉已处理，无法再次审核" });

            // 3. 根据 Action 更新状态
            if (request.Action == "approve")
            {
                appeal.AppealStatus = "approved";
                appeal.ProcessorId = adminId;
                appeal.ProcessTime = DateTime.Now;
                appeal.RejectReason = null; // 审核通过无需拒绝理由
            }
            else if (request.Action == "reject")
            {
                appeal.AppealStatus = "rejected";
                appeal.ProcessorId = adminId;
                appeal.ProcessTime = DateTime.Now;
                appeal.RejectReason = request.RejectReason;
            }
            else
            {
                return Ok(new { code = 400, message = "Action 必须为 approve 或 reject" });
            }

            // 4. 保存事务
            await _db.SaveChangesAsync();

            return Ok(new { code = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理申诉失败，AppealId: {AppealId}", appealId);
            return Ok(new { code = 500, message = "处理失败，请稍后重试" });
        }
    }

    // 批量处理申诉
    [Authorize]
    [HttpPut("batch-process")]
    public async Task<IActionResult> BatchProcessAppeals(
        [FromBody] BatchProcessAppealRequest request
    )
    {
        if (request == null || request.AppealIds == null || request.AppealIds.Count == 0)
            return Ok(new { code = 400, message = "请求体为空或未提供 AppealIds" });

        if (
            string.IsNullOrWhiteSpace(request.Action)
            || (request.Action != "approve" && request.Action != "reject")
        )
            return Ok(new { code = 400, message = "Action 必须为 approve 或 reject" });

        try
        {
            var adminIdStr = User.FindFirst("userId")?.Value;
            if (!int.TryParse(adminIdStr, out int adminId))
                return Ok(new { code = 401, message = "Token 无效或未找到管理员 ID" });

            using var transaction = await _db.Database.BeginTransactionAsync();

            var appeals = await _db
                .AppealSet.Where(a =>
                    request.AppealIds.Contains(a.AppealId) && a.AppealStatus == "pending"
                )
                .ToListAsync();

            foreach (var appeal in appeals)
            {
                appeal.AppealStatus = request.Action == "approve" ? "approved" : "rejected";
                appeal.ProcessorId = adminId;
                appeal.ProcessTime = DateTime.Now;
                appeal.RejectReason = request.Action == "reject" ? request.RejectReason : null;
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { code = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "批量处理申诉失败，AppealIds: {Ids}",
                string.Join(",", request.AppealIds)
            );
            return Ok(new { code = 500, message = "批量处理失败，请稍后重试" });
        }
    }
}
