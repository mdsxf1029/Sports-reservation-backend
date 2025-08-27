using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.RequestModels;
using Sports_reservation_backend.Models.ResponseModels;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[ApiController]
[Route("api/comment-report")]
[SwaggerTag("评论举报相关api")]
public class CommentReportController (OracleDbContext context) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "获取举报表所有数据", Description = "获取举报表所有数据")]
    public async Task<ActionResult<object>> GetAllReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var totalCount = await context.CommentReportSet.CountAsync();
            
            var reports = await (from cr in context.CommentReportSet 
                join reporterUser in context.UserSet on cr.ReporterId equals reporterUser.UserId
                join reportedUser in context.UserSet on cr.ReportedUserId equals reportedUser.UserId
                join reportedComment in context.CommentSet on cr.ReportedCommentId equals reportedComment.CommentId
                orderby cr.ReportId descending
                select new
                {
                    reportId = cr.ReportId,
                    reporterId = cr.ReporterId,
                    reportedUserId = cr.ReportedUserId,
                    reportedCommentId = cr.ReportedCommentId,
                    reportReason = cr.ReportReason,
                    reportTime = cr.ReportTime,
                    reportStatus = cr.ReportStatus,
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
                    reportedComment
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = reports
            });
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
            var report = await context.CommentReportSet
                .Where(pr => pr.ReportId == reportId)
                .Select(pr => new {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedCommentId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus
                })
                .FirstOrDefaultAsync();
            if (report == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }
            
            var reporter = await context.UserSet.Where(u => u.UserId == report.ReporterId)
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
            
            var reportedUser = await context.UserSet.Where(u => u.UserId == report.ReportedUserId)
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
            
            var reportedComment = await context.CommentSet.FindAsync(report.ReportedCommentId);
            if (reportedComment == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }
            
            return Ok(new
            {
                report, reporter,
                reportedUser,
                reportedComment
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("comment/{commentId:int}")]
    [SwaggerOperation(Summary = "根据CommentId获得数据", Description = "获得某评论的所有举报数据")]
    public async Task<ActionResult<object>> GetReportByComment(int commentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var totalCount = await context.CommentReportSet.Where(pr => pr.ReportedCommentId == commentId)
                .CountAsync();
            
            if (totalCount == 0)
            {
                return NotFound($"No corresponding data found for ID: {commentId}");
            }
            
            var reports = await context.CommentReportSet
                .Where(r => r.ReportedCommentId == commentId)
                .Select(pr => new {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedCommentId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus
                })
                .OrderBy(r => r.ReportId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new 
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = reports
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("reporter/{userId:int}")]
    [SwaggerOperation(Summary = "根据UserId获得数据", Description = "获得某用户的所有举报数据")]
    public async Task<ActionResult<object>> GetReportByReporter(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var totalCount = await context.CommentReportSet.Where(pr => pr.ReporterId == userId)
                .CountAsync();
            
            if (totalCount == 0)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }
            
            var reports = await context.CommentReportSet
                .Where(r => r.ReporterId == userId)
                .Select(pr => new {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedCommentId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus
                })
                .OrderBy(r => r.ReportId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new 
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = reports
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("reported/{userId:int}")]
    [SwaggerOperation(Summary = "根据UserId获得数据", Description = "获得某用户的所有被举报数据")]
    public async Task<ActionResult<object>> GetReportByReported(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var totalCount = await context.CommentReportSet.Where(pr => pr.ReportedUserId == userId)
                .CountAsync();
            
            if (totalCount == 0)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }
            
            var reports = await context.CommentReportSet
                .Where(r => r.ReportedUserId == userId)
                .Select(pr => new {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedCommentId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus
                })
                .OrderBy(r => r.ReportId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new 
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = reports
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("checking")]
    [SwaggerOperation(Summary = "获取所有待处理的举报", Description = "获取所有待处理（pending）状态的举报数据")]
    public async Task<ActionResult<object>> GetPendingReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        try
        {
            var query = context.CommentReportSet.Where(r => r.ReportStatus == "checking");
            var totalCount = await query.CountAsync();
            
            var reports = await (from cr in query
                                join reporterUser in context.UserSet on cr.ReporterId equals reporterUser.UserId
                                join reportedUser in context.UserSet on cr.ReportedUserId equals reportedUser.UserId
                                join reportedComment in context.CommentSet on cr.ReportedCommentId equals reportedComment.CommentId
                                orderby cr.ReportId descending
                                select new
                                {
                                    reportId = cr.ReportId,
                                    reporterId = cr.ReporterId,
                                    reportedUserId = cr.ReportedUserId,
                                    reportedCommentId = cr.ReportedCommentId,
                                    reportReason = cr.ReportReason,
                                    reportTime = cr.ReportTime,
                                    reportStatus = cr.ReportStatus,
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
                                    reportedComment
                                })
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = reports
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("accepted")]
    [SwaggerOperation(Summary = "获取所有已接受的举报", Description = "获取所有已接受（accepted）状态的举报数据")]
    public async Task<ActionResult<object>> GetAcceptedReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        try
        {
            var query = context.CommentReportSet.Where(r => r.ReportStatus == "accepted");
            var totalCount = await query.CountAsync();

            var reports = await (from cr in query
                                join reporterUser in context.UserSet on cr.ReporterId equals reporterUser.UserId
                                join reportedUser in context.UserSet on cr.ReportedUserId equals reportedUser.UserId
                                join reportedComment in context.CommentSet on cr.ReportedCommentId equals reportedComment.CommentId
                                orderby cr.ReportId descending
                                select new
                                {
                                    reportId = cr.ReportId,
                                    reporterId = cr.ReporterId,
                                    reportedUserId = cr.ReportedUserId,
                                    reportedCommentId = cr.ReportedCommentId,
                                    reportReason = cr.ReportReason,
                                    reportTime = cr.ReportTime,
                                    reportStatus = cr.ReportStatus,
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
                                    reportedComment
                                })
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = reports
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("rejected")]
    [SwaggerOperation(Summary = "获取所有已拒绝的举报", Description = "获取所有已拒绝（rejected）状态的举报数据")]
    public async Task<ActionResult<object>> GetRejectedReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        try
        {
            var query = context.CommentReportSet.Where(r => r.ReportStatus == "rejected");
            var totalCount = await query.CountAsync();
            
            var reports = await (from cr in query
                                join reporterUser in context.UserSet on cr.ReporterId equals reporterUser.UserId
                                join reportedUser in context.UserSet on cr.ReportedUserId equals reportedUser.UserId
                                join reportedComment in context.CommentSet on cr.ReportedCommentId equals reportedComment.CommentId
                                orderby cr.ReportId descending
                                select new
                                {
                                    reportId = cr.ReportId,
                                    reporterId = cr.ReporterId,
                                    reportedUserId = cr.ReportedUserId,
                                    reportedCommentId = cr.ReportedCommentId,
                                    reportReason = cr.ReportReason,
                                    reportTime = cr.ReportTime,
                                    reportStatus = cr.ReportStatus,
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
                                    reportedComment
                                })
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = reports
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("{commentId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "添加举报表信息", Description = "添加举报表信息")]
    public async Task<IActionResult> AddReport(int commentId, int userId, [FromBody] CommentReport report)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            if (await context.UserSet.FindAsync(userId) == null)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }
            
            var author = await context.UserCommentSet.FindAsync(commentId);
            if (author == null)
            {
                return NotFound($"No corresponding data found for ID: {commentId}");
            }
            
            report.ReporterId = userId;
            report.ReportedCommentId = commentId;
            report.ReportedUserId = author.UserId;
            report.ReportTime = DateTime.Now;
            report.ReportStatus = "checking";
            
            context.CommentReportSet.Add(report);
            await context.SaveChangesAsync();
            
            var validInfo = await context.CommentReportSet
                .Where(pr => pr.ReportId == report.ReportId)
                .Select(pr => new {
                    pr.ReportId,
                    pr.ReporterId,
                    pr.ReportedUserId,
                    pr.ReportedCommentId,
                    pr.ReportReason,
                    pr.ReportTime,
                    pr.ReportStatus
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
            var report = await context.CommentReportSet.FindAsync(reportId);

            if (report == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }

            if (report.ReportStatus != "checking")
            {
                var managerReport = context.CommentReportHandlingSet.Where(mpr => mpr.ReportId == reportId);
                context.CommentReportHandlingSet.RemoveRange(managerReport);
            }
            
            context.CommentReportSet.Remove(report);
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
    public async Task<IActionResult> UpdateReportByUser(int reportId, int userId, [FromBody] CommentReport updatedReport)
    {
        try
        {
            var report = await context.CommentReportSet
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
            
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
 
            context.CommentReportSet.Update(report);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (!context.CommentReportSet.Any(e => e.ReportId == reportId))
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
    public async Task<IActionResult> UpdateReportByManager(int reportId, int userId, [FromBody] ReportUpdateRequest request)
    {
        try
        {
            var report = await context.CommentReportSet
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
            
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
            
            report.ReportStatus = request.Result;
            
            context.CommentReportSet.Update(report);

            var managerReport = new CommentReportHandling
            {
                ReportId = reportId,
                ManagerId = user.UserId,
                ManageTime = DateTime.Now,
                ManageReason = request.Reason,
            };
            
            context.CommentReportHandlingSet.Add(managerReport);
            
            await context.SaveChangesAsync();
            
            return Ok($"Data with ID: {reportId} has been updated successfully.");
        }
        catch (DbUpdateException)
        {
            if (!context.CommentReportSet.Any(e => e.ReportId == reportId))
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