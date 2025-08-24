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
            
            var reports = await context.CommentReportSet
                .OrderBy(r => r.ReportId)
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
    
    [HttpGet("{reportId:int}")]
    [SwaggerOperation(Summary = "根据ReportId获得数据", Description = "获得某一条举报的数据")]
    public async Task<ActionResult<object>> GetReportByPk(int reportId)
    {
        try
        {
            var report = await context.CommentReportSet.FindAsync(reportId);
            
            if (report == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }
            
            var reporter = await context.UserSet.Where(u => u.UserId == report.ReporterId)
                .Select(u => new UserResponse
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Points = u.Points,
                    AvatarUrl = u.AvatarUrl,
                    Gender = u.Gender,
                    Profile = u.Profile,
                    Region = u.Region,
                })
                .FirstOrDefaultAsync();
            if (reporter == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }
            
            var reportedUser = await context.UserSet.Where(u => u.UserId == report.ReportedUserId)
                .Select(u => new UserResponse
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Points = u.Points,
                    AvatarUrl = u.AvatarUrl,
                    Gender = u.Gender,
                    Profile = u.Profile,
                    Region = u.Region,
                })
                .FirstOrDefaultAsync();
            if (reportedUser == null)
            {
                return NotFound($"No corresponding data found for ID: {reportId}");
            }
            
            var reportedComment = await context.PostSet.FindAsync(report.ReportedCommentId);
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
    
    [HttpGet("{commentId:int}")]
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
                .OrderBy(r => r.ReportId)
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
                .OrderBy(r => r.ReportId)
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
                .OrderBy(r => r.ReportId)
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
    
    [HttpPost("{postId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "添加举报表信息", Description = "添加举报表信息")]
    public async Task<IActionResult> AddReport(int postId, int userId, [FromBody] CommentReport report)
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
            
            var author = await context.UserPostSet.FindAsync(postId);

            if (author == null)
            {
                return NotFound($"No corresponding data found for ID: {postId}");
            }
            
            report.ReporterId = userId;
            report.ReportedCommentId = postId;
            report.ReportedUserId = author.UserId;
            report.ReportStatus = "checking";
            
            context.CommentReportSet.Add(report);
            await context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(AddReport), new { id = report.ReportId }, report);
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
                var managerReport = context.ManagerCommentReportSet.Where(mpr => mpr.ReportId == reportId);
                context.ManagerCommentReportSet.RemoveRange(managerReport);
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
            await context.SaveChangesAsync();

            var managerReport = new ManagerCommentReport
            {
                ReportId = reportId,
                ManagerId = user.UserId,
                ManageTime = DateTime.Now,
                ManageReason = request.Reason,
            };
            
            context.ManagerCommentReportSet.Add(managerReport);
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