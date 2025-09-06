using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[ApiController]
[Route("api/appeals")]
[SwaggerTag("申诉管理相关API")]
public class AppealController : ControllerBase
{
    private readonly OracleDbContext _context;
    private readonly ILogger<AppealController> _logger;
    private readonly IConfiguration _config;

    public AppealController(OracleDbContext context, ILogger<AppealController> logger, IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _config = config;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "获取申诉列表", Description = "获取申诉列表，支持分页和筛选")]
    public async Task<IActionResult> GetAppealRecords(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? venue = null,
        [FromQuery] string? processor = null,
        [FromQuery] string? keyword = null)
    {
        try
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = from a in _context.AppealSet
                       join v in _context.ViolationSet on a.ViolationId equals v.ViolationId
                       join uv in _context.UserViolationSet on v.ViolationId equals uv.ViolationId
                       join u in _context.UserSet on uv.UserId equals u.UserId
                       join apt in _context.AppointmentSet on v.AppointmentId equals apt.AppointmentId
                       join va in _context.VenueAppointmentSet on apt.AppointmentId equals va.AppointmentId
                       join ve in _context.VenueSet on va.VenueId equals ve.VenueId
                       join p in _context.UserSet on a.ProcessorId equals p.UserId into pGroup
                       from processor in pGroup.DefaultIfEmpty()
                       select new
                       {
                           id = a.AppealId,
                           violationId = a.ViolationId,
                           userName = u.UserName,
                           userId = u.UserId,
                           userAvatar = u.AvatarUrl,
                           violationTime = v.ViolationTime.HasValue ? v.ViolationTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                           venue = ve.VenueName,
                           timeSlot = apt.BeginTime.HasValue && apt.EndTime.HasValue
                               ? $"{apt.BeginTime.Value:HH:mm}-{apt.EndTime.Value:HH:mm}"
                               : "",
                           appealReason = a.AppealReason,
                           appealTime = a.AppealTime.HasValue ? a.AppealTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                           appealStatus = a.AppealStatus,
                           processTime = a.ProcessTime.HasValue ? a.ProcessTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                           processor = processor != null ? processor.UserName : null,
                           rejectReason = a.RejectReason
                       };

            // 状态筛选
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(x => x.appealStatus == status);
            }

            // 场馆筛选
            if (!string.IsNullOrEmpty(venue))
            {
                query = query.Where(x => x.venue == venue);
            }

            // 处理人筛选
            if (!string.IsNullOrEmpty(processor))
            {
                query = query.Where(x => x.processor == processor);
            }

            // 关键词搜索
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.userName.Contains(keyword) || x.userId.ToString().Contains(keyword));
            }

            var totalCount = await query.CountAsync();
            var appeals = await query
                .OrderByDescending(x => x.appealTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                code = 200,
                msg = "获取申诉列表成功",
                data = appeals,
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取申诉列表失败");
            return Ok(new
            {
                code = 500,
                msg = "获取申诉列表失败",
                data = new object[] { }
            });
        }
    }

    [HttpGet("{appealId}")]
    [SwaggerOperation(Summary = "获取申诉详情", Description = "根据申诉ID获取申诉详情")]
    public async Task<IActionResult> GetAppealDetail(int appealId)
    {
        try
        {
            var appeal = await (from a in _context.AppealSet
                               join v in _context.ViolationSet on a.ViolationId equals v.ViolationId
                               join uv in _context.UserViolationSet on v.ViolationId equals uv.ViolationId
                               join u in _context.UserSet on uv.UserId equals u.UserId
                               join apt in _context.AppointmentSet on v.AppointmentId equals apt.AppointmentId
                               join va in _context.VenueAppointmentSet on apt.AppointmentId equals va.AppointmentId
                               join ve in _context.VenueSet on va.VenueId equals ve.VenueId
                               join p in _context.UserSet on a.ProcessorId equals p.UserId into pGroup
                               from processor in pGroup.DefaultIfEmpty()
                               where a.AppealId == appealId
                               select new
                               {
                                   id = a.AppealId,
                                   violationId = a.ViolationId,
                                   userName = u.UserName,
                                   userId = u.UserId,
                                   userAvatar = u.AvatarUrl,
                                   violationTime = v.ViolationTime.HasValue ? v.ViolationTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                                   venue = ve.VenueName,
                                   timeSlot = apt.BeginTime.HasValue && apt.EndTime.HasValue
                                       ? $"{apt.BeginTime.Value:HH:mm}-{apt.EndTime.Value:HH:mm}"
                                       : "",
                                   appealReason = a.AppealReason,
                                   appealTime = a.AppealTime.HasValue ? a.AppealTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                                   appealStatus = a.AppealStatus,
                                   processTime = a.ProcessTime.HasValue ? a.ProcessTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                                   processor = processor != null ? processor.UserName : null,
                                   rejectReason = a.RejectReason
                               }).FirstOrDefaultAsync();

            if (appeal == null)
            {
                return Ok(new
                {
                    code = 404,
                    msg = "申诉记录不存在",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                code = 200,
                msg = "获取申诉详情成功",
                data = appeal
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取申诉详情失败");
            return Ok(new
            {
                code = 500,
                msg = "获取申诉详情失败",
                data = (object?)null
            });
        }
    }

    [HttpPost]
    [SwaggerOperation(Summary = "提交申诉", Description = "用户提交申诉")]
    public async Task<IActionResult> AddAppeal([FromBody] AddAppealRequest request)
    {
        try
        {
            // 检查违约记录是否存在
            var violation = await _context.ViolationSet.FindAsync(request.ViolationId);
            if (violation == null)
            {
                return Ok(new
                {
                    code = 404,
                    msg = "违约记录不存在",
                    data = (object?)null
                });
            }

            // 检查是否已有申诉
            var existingAppeal = await _context.AppealSet
                .FirstOrDefaultAsync(a => a.ViolationId == request.ViolationId);
            if (existingAppeal != null)
            {
                return Ok(new
                {
                    code = 400,
                    msg = "该违约记录已有申诉",
                    data = (object?)null
                });
            }

            var appeal = new Appeal
            {
                ViolationId = request.ViolationId,
                UserId = request.UserId,
                AppealReason = request.AppealReason,
                AppealTime = DateTime.Now,
                AppealStatus = "pending"
            };

            _context.AppealSet.Add(appeal);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                code = 200,
                msg = "申诉提交成功",
                data = new { appealId = appeal.AppealId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "提交申诉失败");
            return Ok(new
            {
                code = 500,
                msg = "提交申诉失败",
                data = (object?)null
            });
        }
    }

    [HttpPut("{appealId}/process")]
    [SwaggerOperation(Summary = "处理申诉", Description = "管理员处理申诉（通过或拒绝）")]
    public async Task<IActionResult> ProcessAppeal(int appealId, [FromBody] ProcessAppealRequest request)
    {
        try
        {
            // 获取token中的manager_id
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { code = 401, msg = "未授权" });

            string token = authHeader.Substring("Bearer ".Length).Trim();
            var principal = JwtTokenUtil.ValidateToken(token, _config);
            if (principal == null) return Unauthorized(new { code = 401, msg = "Token无效" });

            var managerIdStr = principal.FindFirst("userId")?.Value;
            if (managerIdStr == null) return Unauthorized(new { code = 401, msg = "无法获取管理员ID" });
            int managerId = int.Parse(managerIdStr);

            var appeal = await _context.AppealSet.FindAsync(appealId);
            if (appeal == null)
            {
                return Ok(new
                {
                    code = 404,
                    msg = "申诉记录不存在",
                    data = (object?)null
                });
            }

            if (appeal.AppealStatus != "pending")
            {
                return Ok(new
                {
                    code = 400,
                    msg = "该申诉已被处理",
                    data = (object?)null
                });
            }

            appeal.AppealStatus = request.Action;
            appeal.ProcessTime = DateTime.Now;
            appeal.ProcessorId = managerId;

            if (request.Action == "rejected" && !string.IsNullOrEmpty(request.RejectReason))
            {
                appeal.RejectReason = request.RejectReason;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                code = 200,
                msg = $"申诉{request.Action == "approved" ? "通过" : "拒绝"}成功",
                data = new { appealId = appeal.AppealId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理申诉失败");
            return Ok(new
            {
                code = 500,
                msg = "处理申诉失败",
                data = (object?)null
            });
        }
    }

    [HttpPut("batch-process")]
    [SwaggerOperation(Summary = "批量处理申诉", Description = "管理员批量处理申诉")]
    public async Task<IActionResult> BatchProcessAppeals([FromBody] BatchProcessAppealRequest request)
    {
        try
        {
            // 获取token中的manager_id
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { code = 401, msg = "未授权" });

            string token = authHeader.Substring("Bearer ".Length).Trim();
            var principal = JwtTokenUtil.ValidateToken(token, _config);
            if (principal == null) return Unauthorized(new { code = 401, msg = "Token无效" });

            var managerIdStr = principal.FindFirst("userId")?.Value;
            if (managerIdStr == null) return Unauthorized(new { code = 401, msg = "无法获取管理员ID" });
            int managerId = int.Parse(managerIdStr);

            var appeals = await _context.AppealSet
                .Where(a => request.AppealIds.Contains(a.AppealId) && a.AppealStatus == "pending")
                .ToListAsync();

            foreach (var appeal in appeals)
            {
                appeal.AppealStatus = request.Action;
                appeal.ProcessTime = DateTime.Now;
                appeal.ProcessorId = managerId;

                if (request.Action == "rejected" && !string.IsNullOrEmpty(request.RejectReason))
                {
                    appeal.RejectReason = request.RejectReason;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                code = 200,
                msg = $"批量{request.Action == "approved" ? "通过" : "拒绝"}申诉成功",
                data = new { processedCount = appeals.Count }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量处理申诉失败");
            return Ok(new
            {
                code = 500,
                msg = "批量处理申诉失败",
                data = (object?)null
            });
        }
    }
}
