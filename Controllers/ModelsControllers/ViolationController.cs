using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Sports_reservation_backend.Models.RequestModels;
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
    [HttpGet]
    public async Task<IActionResult> GetViolationList()
    {
        try
        {
            var now = DateTime.Now;

            var violations = await (
                from v in _db.ViolationSet
                join uv in _db.UserViolationSet on v.ViolationId equals uv.ViolationId
                join u in _db.UserSet on uv.UserId equals u.UserId
                join a in _db.AppointmentSet on v.AppointmentId equals a.AppointmentId
                join va in _db.VenueAppointmentSet on a.AppointmentId equals va.AppointmentId
                join ve in _db.VenueSet on va.VenueId equals ve.VenueId
                join b in _db.BlacklistSet.Where(bl => bl.BannedStatus == "valid"
                                                       && bl.BeginTime <= now
                                                       && bl.EndTime >= now)
                     on u.UserId equals b.UserId into blGroup
                from b2 in blGroup.DefaultIfEmpty() // 左连接，可能没有有效黑名单
                select new
                {
                    id = v.ViolationId,
                    userName = u.UserName,
                    userId = u.UserId,
                    violationReason = v.ViolationReason,
                    venue = ve.VenueName,
                    timeSlot = a.BeginTime.HasValue && a.EndTime.HasValue
                        ? $"{a.BeginTime.Value:HH:mm}-{a.EndTime.Value:HH:mm}"
                        : "",
                    violationDate = v.ViolationTime.HasValue ? v.ViolationTime.Value.ToString("yyyy-MM-dd") : "",
                    isBlacklisted = b2 != null,
                    blacklistTimestamp = b2 != null ? (DateTime?)b2.BeginTime : null
                }
            ).ToListAsync();

            return Ok(new
            {
                success = true,
                data = violations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取违约记录列表失败");
            return Ok(new
            {
                success = false,
                data = new object[] { }
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddViolation([FromBody] AddViolationRequest request)
    {
        try
        {
            // 1. 查找用户和预约
            var user = await _db.UserSet.FindAsync(request.UserId);
            if (user == null) return BadRequest(new { success = false, message = "用户不存在" });

            var appointment = await _db.AppointmentSet.FindAsync(request.AppointmentId);
            if (appointment == null) return BadRequest(new { success = false, message = "预约不存在" });

            // 2. 创建违约记录
            var violation = new Violation
            {
                AppointmentId = request.AppointmentId,
                ViolationReason = request.ViolationReason,
                ViolationPenalty = request.ViolationPenalty,
                ViolationTime = DateTime.Now
            };
            _db.ViolationSet.Add(violation);
            await _db.SaveChangesAsync(); // 获取 violation_id

            // 3. 创建 user_violation 关联
            var userViolation = new UserViolation
            {
                UserId = request.UserId,
                ViolationId = violation.ViolationId
            };
            _db.UserViolationSet.Add(userViolation);
            await _db.SaveChangesAsync();

            // 4. 查询场地信息
            var venueName = await (
                from va in _db.VenueAppointmentSet
                join ve in _db.VenueSet on va.VenueId equals ve.VenueId
                where va.AppointmentId == request.AppointmentId
                select ve.VenueName
            ).FirstOrDefaultAsync();

            // 5. 查询是否在黑名单
            var now = DateTime.Now;
            var blacklist = await _db.BlacklistSet
                .Where(b => b.UserId == request.UserId
                            && b.BannedStatus == "valid"
                            && b.BeginTime <= now
                            && b.EndTime >= now)
                .FirstOrDefaultAsync();

            // 6. 返回结果
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
                isBlacklisted = blacklist != null,
                blacklistTimestamp = blacklist?.BeginTime
            };

            return Ok(new
            {
                success = true,
                data = result,
                message = "创建违约记录成功"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加违约记录失败");
            return Ok(new
            {
                success = false,
                message = "添加违约记录失败",
                data = (object?)null
            });
        }
    }

}
