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
            // 1. 基础查询：只查询 Appeal 表，后续通过关联字段查询相关数据
            var query = _db
                .AppealSet.Join(
                    _db.ViolationSet,
                    a => a.ViolationId,
                    v => v.ViolationId,
                    (a, v) => new { a, v }
                )
                .Join(
                    _db.UserSet,
                    av => av.a.UserId,
                    u => u.UserId,
                    (av, u) =>
                        new
                        {
                            av.a,
                            av.v,
                            u,
                        }
                )
                .Join(
                    _db.VenueAppointmentSet,
                    avu => avu.v.AppointmentId,
                    va => va.AppointmentId,
                    (avu, va) =>
                        new
                        {
                            avu.a,
                            avu.v,
                            avu.u,
                            va,
                        }
                )
                .Join(
                    _db.VenueSet,
                    avuva => avuva.va.VenueId,
                    ve => ve.VenueId,
                    (avuva, ve) =>
                        new
                        {
                            avuva.a,
                            avuva.v,
                            avuva.u,
                            avuva.va,
                            ve,
                        }
                )
                .Join(
                    _db.AppointmentSet,
                    avuve => avuve.va.AppointmentId,
                    app => app.AppointmentId,
                    (avuve, app) =>
                        new
                        {
                            avuve.a,
                            avuve.v,
                            avuve.u,
                            avuve.va,
                            avuve.ve,
                            app,
                        }
                )
                .Join(
                    _db.TimeSlotSet,
                    avuvea => avuvea.app.BeginTime,
                    ts => ts.BeginTime,
                    (avuvea, ts) =>
                        new
                        {
                            avuvea.a,
                            avuvea.v,
                            avuvea.u,
                            avuvea.va,
                            avuvea.ve,
                            avuvea.app,
                            ts,
                        }
                )
                .GroupJoin(
                    _db.UserSet,
                    avuvea => avuvea.a.ProcessorId,
                    pu => pu.UserId,
                    (avuvea, processorJoin) =>
                        new
                        {
                            avuvea.a,
                            avuvea.v,
                            avuvea.u,
                            avuvea.va,
                            avuvea.ve,
                            avuvea.app,
                            avuvea.ts,
                            processorJoin,
                        }
                )
                .SelectMany(
                    avuvea => avuvea.processorJoin.DefaultIfEmpty(),
                    (avuvea, processorUser) =>
                        new
                        {
                            Id = avuvea.a.AppealId,
                            UserName = avuvea.u.UserName,
                            UserId = avuvea.u.UserId,
                            UserAvatar = avuvea.u.AvatarUrl,
                            ViolationTime = avuvea.v.ViolationTime,
                            Venue = avuvea.ve.VenueName,
                            TimeSlot = avuvea.ts.BeginTime + "-" + avuvea.ts.EndTime,
                            AppealReason = avuvea.a.AppealReason,
                            AppealTime = avuvea.a.AppealTime,
                            AppealStatus = avuvea.a.AppealStatus,
                            Processor = avuvea.a.ProcessorId,
                            ProcessorName = processorUser != null ? processorUser.UserName : null,
                            ProcessTime = avuvea.a.ProcessTime,
                        }
                );

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
            // 先获取申诉基本信息
            var appeal = await _db
                .AppealSet.Where(a => a.AppealId == appealId)
                .Select(a => new
                {
                    a.AppealId,
                    a.AppealReason,
                    a.AppealTime,
                    a.AppealStatus,
                    a.ProcessTime,
                    a.ProcessorId,
                    a.ViolationId,
                    a.UserId,
                })
                .FirstOrDefaultAsync();

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

            // 获取违约记录（Violation）信息
            var violation = await _db
                .ViolationSet.Where(v => v.ViolationId == appeal.ViolationId)
                .Select(v => new { v.AppointmentId, v.ViolationTime })
                .FirstOrDefaultAsync();

            if (violation == null)
            {
                return Ok(
                    new
                    {
                        code = 404,
                        message = "违约记录不存在",
                        data = (object?)null,
                    }
                );
            }

            // 获取预约（Appointment）信息
            var appointment = await _db
                .AppointmentSet.Where(app => app.AppointmentId == violation.AppointmentId)
                .Select(app => new { app.BeginTime, app.EndTime })
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return Ok(
                    new
                    {
                        code = 404,
                        message = "预约记录不存在",
                        data = (object?)null,
                    }
                );
            }

            // 获取时间段（TimeSlot）信息
            var timeSlot = await _db
                .TimeSlotSet.Where(ts => ts.BeginTime == appointment.BeginTime)
                .Select(ts => new { ts.BeginTime, ts.EndTime })
                .FirstOrDefaultAsync();

            if (timeSlot == null)
            {
                return Ok(
                    new
                    {
                        code = 404,
                        message = "时间段不存在",
                        data = (object?)null,
                    }
                );
            }

            // 获取用户信息（User）
            var user = await _db
                .UserSet.Where(u => u.UserId == appeal.UserId)
                .Select(u => new { u.UserName, u.AvatarUrl })
                .FirstOrDefaultAsync();

            // 获取处理人信息（Processor）
            var processor = await _db
                .UserSet.Where(u => u.UserId == appeal.ProcessorId)
                .Select(u => new { u.UserName })
                .FirstOrDefaultAsync();

            // 组装最终返回的数据
            var result = new
            {
                appeal.AppealId,
                UserName = user?.UserName,
                UserAvatar = user?.AvatarUrl,
                ViolationTime = violation.ViolationTime,
                TimeSlot = $"{timeSlot.BeginTime} - {timeSlot.EndTime}",
                AppealReason = appeal.AppealReason,
                AppealTime = appeal.AppealTime,
                AppealStatus = appeal.AppealStatus,
                ProcessorName = processor?.UserName,
                ProcessTime = appeal.ProcessTime,
            };

            return Ok(new { code = 200, data = result });
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
