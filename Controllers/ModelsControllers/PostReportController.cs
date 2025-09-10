using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Models.ResponseModels;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[ApiController]
[Route("api/post-report")]
[SwaggerTag("帖子举报相关api")]
public class PostReportController(OracleDbContext context) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "获取举报表所有数据", Description = "获取举报表所有数据")]
    public async Task<ActionResult<object>> GetAllReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        page = page < 1 ? 1 : page;

        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            var totalCount = await context.PostReportSet.CountAsync();

            var reports = await (
                from pr in context.PostReportSet
                join reporterUser in context.UserSet on pr.ReporterId equals reporterUser.UserId
                join reportedUser in context.UserSet on pr.ReportedUserId equals reportedUser.UserId
                join reportedPost in context.PostSet on pr.ReportedPostId equals reportedPost.PostId
                orderby pr.ReportId descending
                select new
                {
                    reportId = pr.ReportId,
                    reporterId = pr.ReporterId,
                    reportedUserId = pr.ReportedUserId,
                    reportedPostId = pr.ReportedPostId,
                    reportReason = pr.ReportReason,
                    reportTime = pr.ReportTime,
                    reportStatus = pr.ReportStatus,
                    reporter = new
                    {
                        userId = reporterUser.UserId,
                        username = reporterUser.UserName,
                        points = reporterUser.Points,
                        avatarUrl = reporterUser.AvatarUrl,
                        gender = reporterUser.Gender,
                        profile = reporterUser.Profile,
                        region = reporterUser.Region,
                    },
                    reportedUser = new
                    {
                        userId = reportedUser.UserId,
                        username = reportedUser.UserName,
                        points = reportedUser.Points,
                        avatarUrl = reportedUser.AvatarUrl,
                        gender = reportedUser.Gender,
                        profile = reportedUser.Profile,
                        region = reportedUser.Region,
                    },
                    reportedPost,
                }
            )
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    list = reports,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{reportId:int}")]
    [SwaggerOperation(Summary = "根据ReportId获得数据", Description = "获得某一条举报的数据")]
    public async Task<ActionResult<object>> GetReportByPk(int reportId)
    {
        try
        {
            var report = await context
                .PostReportSet.Where(pr => pr.ReportId == reportId)
                .Select(pr => new
                {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedPostId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus,
                })
                .FirstOrDefaultAsync();
            if (report == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }

            var reporter = await context
                .UserSet.Where(u => u.UserId == report.ReporterId)
                .Select(u => new UserResponse
                {
                    userId = u.UserId,
                    username = u.UserName,
                    points = u.Points,
                    avatarUrl = u.AvatarUrl,
                    gender = u.Gender,
                    profile = u.Profile,
                    region = u.Region,
                })
                .FirstOrDefaultAsync();
            if (reporter == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }

            var reportedUser = await context
                .UserSet.Where(u => u.UserId == report.ReportedUserId)
                .Select(u => new UserResponse
                {
                    userId = u.UserId,
                    username = u.UserName,
                    points = u.Points,
                    avatarUrl = u.AvatarUrl,
                    gender = u.Gender,
                    profile = u.Profile,
                    region = u.Region,
                })
                .FirstOrDefaultAsync();
            if (reportedUser == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }

            var reportedPost = await context.PostSet.FindAsync(report.ReportedPostId);
            if (reportedPost == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }

            return Ok(
                new
                {
                    report,
                    reporter,
                    reportedUser,
                    reportedPost,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("post/{postId:int}")]
    [SwaggerOperation(Summary = "根据PostId获得数据", Description = "获得某帖子的所有举报数据")]
    public async Task<ActionResult<object>> GetReportByPost(
        int postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        page = page < 1 ? 1 : page;

        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            var totalCount = await context
                .PostReportSet.Where(pr => pr.ReportedPostId == postId)
                .CountAsync();

            if (totalCount == 0)
            {
                return NotFound($"No corresponding data found for ID: {postId}");
            }

            var reports = await context
                .PostReportSet.Where(r => r.ReportedPostId == postId)
                .Select(pr => new
                {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedPostId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus,
                })
                .OrderBy(r => r.ReportId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    data = reports,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("reporter/{userId:int}")]
    [SwaggerOperation(Summary = "根据UserId获得数据", Description = "获得某用户的所有举报数据")]
    public async Task<ActionResult<object>> GetReportByReporter(
        int userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        page = page < 1 ? 1 : page;

        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            var totalCount = await context
                .PostReportSet.Where(pr => pr.ReporterId == userId)
                .CountAsync();

            if (totalCount == 0)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }

            var reports = await context
                .PostReportSet.Where(r => r.ReporterId == userId)
                .Select(pr => new
                {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedPostId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus,
                })
                .OrderBy(r => r.ReportId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    data = reports,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("reported/{userId:int}")]
    [SwaggerOperation(Summary = "根据UserId获得数据", Description = "获得某用户的所有被举报数据")]
    public async Task<ActionResult<object>> GetReportByReported(
        int userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        page = page < 1 ? 1 : page;

        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            var totalCount = await context
                .PostReportSet.Where(pr => pr.ReportedUserId == userId)
                .CountAsync();

            if (totalCount == 0)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }

            var reports = await context
                .PostReportSet.Where(r => r.ReportedUserId == userId)
                .Select(pr => new
                {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedPostId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus,
                })
                .OrderBy(r => r.ReportId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    data = reports,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("checking")]
    [SwaggerOperation(
        Summary = "获取所有待处理的举报",
        Description = "获取所有待处理（checking）状态的举报数据"
    )]
    public async Task<ActionResult<object>> GetCheckingReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null
    )
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        try
        {
            var query = context.PostReportSet.Where(r => r.ReportStatus == "checking");

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.ReportReason.Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var reports = await (
                from pr in query
                join reporterUser in context.UserSet on pr.ReporterId equals reporterUser.UserId
                join reportedUser in context.UserSet on pr.ReportedUserId equals reportedUser.UserId
                join reportedPost in context.PostSet on pr.ReportedPostId equals reportedPost.PostId
                orderby pr.ReportId descending
                select new
                {
                    reportId = pr.ReportId,
                    reporterId = pr.ReporterId,
                    reportedUserId = pr.ReportedUserId,
                    reportedPostId = pr.ReportedPostId,
                    reportReason = pr.ReportReason,
                    reportTime = pr.ReportTime,
                    reportStatus = pr.ReportStatus,
                    reporter = new
                    {
                        userId = reporterUser.UserId,
                        username = reporterUser.UserName,
                        points = reporterUser.Points,
                        avatarUrl = reporterUser.AvatarUrl,
                        gender = reporterUser.Gender,
                        profile = reporterUser.Profile,
                        region = reporterUser.Region,
                    },
                    reportedUser = new
                    {
                        userId = reportedUser.UserId,
                        username = reportedUser.UserName,
                        points = reportedUser.Points,
                        avatarUrl = reportedUser.AvatarUrl,
                        gender = reportedUser.Gender,
                        profile = reportedUser.Profile,
                        region = reportedUser.Region,
                    },
                    reportedPost,
                }
            )
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    list = reports,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("accepted")]
    [SwaggerOperation(
        Summary = "获取所有已接受的举报",
        Description = "获取所有已接受（accepted）状态的举报数据"
    )]
    public async Task<ActionResult<object>> GetAcceptedReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null
    )
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        try
        {
            var query = context.PostReportSet.Where(r => r.ReportStatus == "accepted");

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.ReportReason.Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var reports = await (
                from pr in query
                join reporterUser in context.UserSet on pr.ReporterId equals reporterUser.UserId
                join reportedUser in context.UserSet on pr.ReportedUserId equals reportedUser.UserId
                join reportedPost in context.PostSet on pr.ReportedPostId equals reportedPost.PostId
                orderby pr.ReportId descending
                select new
                {
                    reportId = pr.ReportId,
                    reporterId = pr.ReporterId,
                    reportedUserId = pr.ReportedUserId,
                    reportedPostId = pr.ReportedPostId,
                    reportReason = pr.ReportReason,
                    reportTime = pr.ReportTime,
                    reportStatus = pr.ReportStatus,
                    reporter = new
                    {
                        userId = reporterUser.UserId,
                        username = reporterUser.UserName,
                        points = reporterUser.Points,
                        avatarUrl = reporterUser.AvatarUrl,
                        gender = reporterUser.Gender,
                        profile = reporterUser.Profile,
                        region = reporterUser.Region,
                    },
                    reportedUser = new
                    {
                        userId = reportedUser.UserId,
                        username = reportedUser.UserName,
                        points = reportedUser.Points,
                        avatarUrl = reportedUser.AvatarUrl,
                        gender = reportedUser.Gender,
                        profile = reportedUser.Profile,
                        region = reportedUser.Region,
                    },
                    reportedPost,
                }
            )
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    list = reports,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 获取所有已拒绝（rejected）的举报
    [HttpGet("rejected")]
    [SwaggerOperation(
        Summary = "获取所有已拒绝的举报",
        Description = "获取所有已拒绝（rejected）状态的举报数据"
    )]
    public async Task<ActionResult<object>> GetRejectedReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null
    )
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        try
        {
            var query = context.PostReportSet.Where(r => r.ReportStatus == "rejected");

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.ReportReason.Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var reports = await (
                from pr in query
                join reporterUser in context.UserSet on pr.ReporterId equals reporterUser.UserId
                join reportedUser in context.UserSet on pr.ReportedUserId equals reportedUser.UserId
                join reportedPost in context.PostSet on pr.ReportedPostId equals reportedPost.PostId
                orderby pr.ReportId descending
                select new
                {
                    reportId = pr.ReportId,
                    reporterId = pr.ReporterId,
                    reportedUserId = pr.ReportedUserId,
                    reportedPostId = pr.ReportedPostId,
                    reportReason = pr.ReportReason,
                    reportTime = pr.ReportTime,
                    reportStatus = pr.ReportStatus,
                    reporter = new
                    {
                        userId = reporterUser.UserId,
                        username = reporterUser.UserName,
                        points = reporterUser.Points,
                        avatarUrl = reporterUser.AvatarUrl,
                        gender = reporterUser.Gender,
                        profile = reporterUser.Profile,
                        region = reporterUser.Region,
                    },
                    reportedUser = new
                    {
                        userId = reportedUser.UserId,
                        username = reportedUser.UserName,
                        points = reportedUser.Points,
                        avatarUrl = reportedUser.AvatarUrl,
                        gender = reportedUser.Gender,
                        profile = reportedUser.Profile,
                        region = reportedUser.Region,
                    },
                    reportedPost,
                }
            )
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    list = reports,
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("{postId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "添加举报表信息", Description = "添加举报表信息")]
    public async Task<IActionResult> AddReport(int postId, int userId, [FromBody] PostReport report)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var reportedPost = await context.PostSet.FirstOrDefaultAsync(p => p.PostId == postId);
            if (reportedPost == null)
            {
                return NotFound($"No corresponding data found for ID: {postId}");
            }

            var reporter = await context.UserSet.FirstOrDefaultAsync(u => u.UserId == userId);
            if (reporter == null)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }

            var userPost = await context.UserPostSet.FindAsync(postId);
            if (userPost == null)
            {
                return NotFound($"No corresponding data found for ID: {postId}");
            }

            report.ReporterId = userId;
            report.ReportedPostId = postId;
            report.ReportedUserId = userPost.UserId;
            report.ReportTime = DateTime.UtcNow.AddHours(8);
            report.ReportStatus = "checking";

            context.PostReportSet.Add(report);
            await context.SaveChangesAsync();

            var validInfo = await context
                .PostReportSet.Where(pr => pr.ReportId == report.ReportId)
                .Select(pr => new
                {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedPostId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus,
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(AddReport), new { id = report.ReportId }, validInfo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{reportId:int}")]
    [SwaggerOperation(Summary = "删除举报表信息", Description = "删除举报表信息")]
    public async Task<IActionResult> DeleteReport(int reportId)
    {
        try
        {
            var report = await context.PostReportSet.FindAsync(reportId);

            if (report == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }

            if (report.ReportStatus != "checking")
            {
                var managerReport = context.PostReportHandlingSet.Where(mpr =>
                    mpr.ReportId == reportId
                );
                context.PostReportHandlingSet.RemoveRange(managerReport);
            }

            context.PostReportSet.Remove(report);
            await context.SaveChangesAsync();

            return Ok($"Data with ID: {reportId} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{reportId:int}/user/{userId:int}")]
    [SwaggerOperation(Summary = "更新举报表信息", Description = "用户更新举报信息")]
    public async Task<IActionResult> UpdateReportByUser(
        int reportId,
        int userId,
        [FromBody] PostReport updatedReport
    )
    {
        try
        {
            var report = await context.PostReportSet.FirstOrDefaultAsync(r =>
                r.ReportId == reportId
            );

            if (report == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }

            if (report.ReporterId != userId)
            {
                return BadRequest($"ReporterId and userId: {userId} mismatch");
            }

            if (report.ReportStatus != "checking")
            {
                return BadRequest($"Report status: {report.ReportStatus} cannot be modified");
            }

            report.ReportReason = updatedReport.ReportReason;

            context.PostReportSet.Update(report);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (!context.PostReportSet.Any(e => e.ReportId == reportId))
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return Ok($"Data with ID: {reportId} has been updated successfully.");
    }

    [HttpPut("{reportId:int}/manager/{userId:int}")]
    [SwaggerOperation(Summary = "更新举报表信息", Description = "管理员处理举报")]
    public async Task<IActionResult> UpdateReportByManager(
        int reportId,
        int userId,
        [FromBody] ReportUpdateRequest request
    )
    {
        try
        {
            var report = await context.PostReportSet.FirstOrDefaultAsync(r =>
                r.ReportId == reportId
            );

            if (report == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }

            if (report.ReportStatus != "checking")
            {
                return BadRequest($"Report status: {report.ReportStatus} cannot be modified");
            }

            var user = await context.UserSet.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }

            if (user.Role != "manager")
            {
                return BadRequest($"User: {userId} does not have permission to modify");
            }

            // 更新举报状态
            report.ReportStatus = request.Result;
            context.PostReportSet.Update(report);

            // 记录管理员的处理操作
            var managerReport = new PostReportHandling
            {
                ReportId = reportId,
                ManagerId = user.UserId,
                ManageTime = DateTime.UtcNow.AddHours(8),
                ManageReason = request.Reason,
            };
            context.PostReportHandlingSet.Add(managerReport);

            // ====== 新增通知 & 修改帖子状态 ======
            var reporterId = report.ReporterId; // 举报人
            var reportedUserId = report.ReportedUserId; // 被举报人
            var reportedUser = await context.UserSet.FirstOrDefaultAsync(u =>
                u.UserId == reportedUserId
            );

            if (request.Result == "rejected")
            {
                // 举报被拒绝 -> 给举报人发通知
                var notification = new Notification
                {
                    UserId = reporterId,
                    Content =
                        $"您对用户 {reportedUser?.UserName} (id:{reportedUserId}) 的举报未被管理员接受",
                    IsRead = 0,
                    CreateTime = DateTime.UtcNow.AddHours(8),
                };
                context.NotificationSet.Add(notification);
            }
            else if (request.Result == "accepted")
            {
                // 找到被举报的帖子
                var post = await context.PostSet.FirstOrDefaultAsync(p =>
                    p.PostId == report.ReportedPostId
                );

                // 举报被接受 -> 给举报人发通知
                var notificationReporter = new Notification
                {
                    UserId = reporterId,
                    Content =
                        $"您对用户 {reportedUser?.UserName} (id:{reportedUserId}) 的举报已被管理员接受，对应帖子已删除",
                    IsRead = 0,
                    CreateTime = DateTime.UtcNow.AddHours(8),
                };
                context.NotificationSet.Add(notificationReporter);

                // 给被举报人发通知 & 修改帖子状态
                if (post != null)
                {
                    var notificationReported = new Notification
                    {
                        UserId = reportedUserId,
                        Content = $"您的帖子‘{post.PostContent}’已被举报删除",
                        IsRead = 0,
                        CreateTime = DateTime.UtcNow.AddHours(8),
                    };
                    context.NotificationSet.Add(notificationReported);

                    // 修改帖子状态为 private
                    post.PostStatus = "private";
                    context.PostSet.Update(post);
                }
            }

            await context.SaveChangesAsync();

            return Ok($"Data with ID: {reportId} has been updated successfully.");
        }
        catch (DbUpdateException)
        {
            if (!context.PostReportSet.Any(e => e.ReportId == reportId))
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
