using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers;

[ApiController]
[Route("api/violations")]
public class ViolationController : ControllerBase
{
    private readonly OracleDbContext _db;
    private readonly ILogger<ViolationController> _logger;

    public ViolationController(OracleDbContext db, ILogger<ViolationController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("violation-list")]
    public async Task<IActionResult> GetViolationList(
        int page = 1,
        int pageSize = 10,
        string keyword = null
    )
    {
        try
        {
            if (page <= 0)
                page = 1;
            if (pageSize <= 0)
                pageSize = 10;

            // 1. 基础查询
            var query =
                from v in _db.ViolationSet
                join uv in _db.UserViolationSet on v.ViolationId equals uv.ViolationId
                join u in _db.UserSet on uv.UserId equals u.UserId
                join a in _db.AppointmentSet on v.AppointmentId equals a.AppointmentId
                join va in _db.VenueAppointmentSet on a.AppointmentId equals va.AppointmentId
                join ve in _db.VenueSet on va.VenueId equals ve.VenueId
                select new
                {
                    v.ViolationId,
                    u.UserName,
                    u.UserId,
                    v.ViolationReason,
                    ve.VenueName,
                    a.BeginTime,
                    a.EndTime,
                    v.ViolationTime,
                    // 是否有任意有效黑名单
                    IsBlacklisted = _db.BlacklistSet.Any(bl =>
                        bl.UserId == u.UserId && bl.BannedStatus == "valid"
                    ),
                    // 最新一条有效黑名单开始时间
                    BlacklistTimestamp = _db
                        .BlacklistSet.Where(bl =>
                            bl.UserId == u.UserId && bl.BannedStatus == "valid"
                        )
                        .OrderByDescending(bl => bl.BeginTime)
                        .Select(bl => (DateTime?)bl.BeginTime)
                        .FirstOrDefault(),
                };

            // 2. 关键字搜索
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(q =>
                    q.UserName.Contains(keyword) || q.UserId.ToString().Contains(keyword)
                );
            }

            // 3. 总记录数
            var totalCount = await query.CountAsync();

            // 4. 分页并按违约时间降序
            var list = await query
                .OrderByDescending(q => q.ViolationTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. 内存中格式化输出
            var violations = list.Select(v => new
                {
                    id = v.ViolationId,
                    userName = v.UserName,
                    userId = v.UserId,
                    violationReason = v.ViolationReason,
                    venue = v.VenueName,
                    timeSlot = v.BeginTime.HasValue && v.EndTime.HasValue
                        ? $"{v.BeginTime.Value:HH:mm}-{v.EndTime.Value:HH:mm}"
                        : "",
                    violationDate = v.ViolationTime?.ToString("yyyy-MM-dd"),
                    isBlacklisted = v.IsBlacklisted,
                    blacklistTimestamp = v.BlacklistTimestamp,
                })
                .ToList();

            // 6. 返回
            return Ok(
                new
                {
                    success = true,
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    data = violations,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取违约记录列表失败");
            return Ok(new { success = false, data = new object[] { } });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddViolation([FromBody] AddViolationRequest request)
    {
        try
        {
            // 1. 查找用户和预约
            var user = await _db.UserSet.FindAsync(request.UserId);
            if (user == null)
                return BadRequest(new { success = false, message = "用户不存在" });

            var appointment = await _db.AppointmentSet.FindAsync(request.AppointmentId);
            if (appointment == null)
                return BadRequest(new { success = false, message = "预约不存在" });

            var now = DateTime.UtcNow.AddHours(8);

            // 2. 创建违约记录
            var violation = new Violation
            {
                AppointmentId = request.AppointmentId,
                ViolationReason = request.ViolationReason,
                ViolationPenalty = request.ViolationPenalty,
                ViolationTime = now,
            };
            _db.ViolationSet.Add(violation);
            await _db.SaveChangesAsync(); // 保存后获取 ViolationId

            // 3. 创建 user_violation 关联
            var userViolation = new UserViolation
            {
                UserId = request.UserId,
                ViolationId = violation.ViolationId,
            };
            _db.UserViolationSet.Add(userViolation);

            // 4. 查询场地信息
            var venueName = await (
                from va in _db.VenueAppointmentSet
                join ve in _db.VenueSet on va.VenueId equals ve.VenueId
                where va.AppointmentId == request.AppointmentId
                select ve.VenueName
            ).FirstOrDefaultAsync();

            // 5. 查询是否在有效黑名单
            bool isBlacklisted = await _db.BlacklistSet.AnyAsync(b =>
                b.UserId == request.UserId && b.BannedStatus == "valid"
            );

            // 6. 保存 user_violation
            await _db.SaveChangesAsync();

            // 7. 构造返回结果
            var result = new
            {
                id = violation.ViolationId,
                userName = user.UserName,
                userId = user.UserId,
                violationReason = violation.ViolationReason,
                venue = venueName,
                timeSlot = appointment.BeginTime.HasValue && appointment.EndTime.HasValue
                    ? $"{appointment.BeginTime.Value:HH:mm}-{appointment.EndTime.Value:HH:mm}"
                    : "",
                violationDate = violation.ViolationTime?.ToString("yyyy-MM-dd"),
                isBlacklisted = isBlacklisted,
            };

            return Ok(
                new
                {
                    success = true,
                    data = result,
                    message = "创建违约记录成功",
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加违约记录失败");
            return Ok(
                new
                {
                    success = false,
                    message = "添加违约记录失败",
                    data = (object?)null,
                }
            );
        }
    }
}
