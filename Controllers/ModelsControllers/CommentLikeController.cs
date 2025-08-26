using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.ResponseModels;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/comment-like")]
[ApiController]
[SwaggerTag("评论点赞相关api")]
public class CommentLikeController(OracleDbContext context) : ControllerBase
{
    [HttpGet("comment/{commentId:int}")]
    [SwaggerOperation(Summary = "根据评论ID获取点赞其的用户", Description = "根据评论ID获取点赞其的用户")]
    public async Task<ActionResult<object>> GetUsersByComment(int commentId)
    {
        try
        {
            var userIds = await context.CommentLikeSet
                .Where(p => p.CommentId == commentId)
                .Select(pl => pl.UserId)
                .ToListAsync();

            if (userIds.Count == 0)
            {
                return NotFound($"No corresponding data found for ID: {commentId}");
            }

            var users = await context.UserSet
                .Where(u => userIds.Contains(u.UserId))
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
                .ToListAsync();

            return Ok(new { count = users.Count, data = users });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "根据用户ID获取其点赞的评论", Description = "根据用户ID获取其点赞的评论")]
    public async Task<ActionResult<object>> GetCommentsByUser(int userId)
    {
        try
        {
            var comments = await context.CommentLikeSet
                .Where(cl => cl.UserId == userId)
                .Include(cl => cl.Comment)
                .Select(cl => cl.Comment)
                .ToListAsync();

            return Ok(new { count = comments.Count, data = comments });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("{commentId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "点赞评论", Description = "点赞评论")]
    public async Task<IActionResult> LikeComment(int commentId, int userId)
    {
        var exists = await context.CommentLikeSet
            .AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

        if (exists)
        {
            return BadRequest($"User has already liked this comment.");
        }

        var like = new CommentLike
        {
            CommentId = commentId,
            UserId = userId,
            LikeTime = DateTime.Now
        };

        context.CommentLikeSet.Add(like);
        
        var comment = await context.CommentSet.FindAsync(commentId);
        if (comment != null)
        {
            comment.LikeCount++;
        }

        await context.SaveChangesAsync();
        return Ok($"Comment with ID: {commentId} has been liked successfully by user: {userId}.");
    }
        
    
    [HttpDelete("{commentId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "取消点赞", Description = "取消点赞")]
    public async Task<IActionResult> UnlikeComment(int commentId, int userId)
    {
        var like = await context.CommentLikeSet
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

        if (like == null)
        {
            return NotFound($"No corresponding like found for comment ID: {commentId} and user ID: {userId}.");
        }

        context.CommentLikeSet.Remove(like);
        
        var comment = await context.CommentSet.FindAsync(commentId);
        if (comment != null && comment.LikeCount > 0)
        {
            comment.LikeCount--;
        }

        await context.SaveChangesAsync();
        
        return Ok($"Like for comment ID: {commentId} by user: {userId} has been successfully removed.");
    }
}