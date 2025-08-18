using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/comment-dislike")]
[ApiController]
[SwaggerTag("评论点踩相关api")]
public class CommentDislikeController(OracleDbContext context) : ControllerBase
{
    [HttpGet("comment/{commentId:int}")]
    [SwaggerOperation(Summary = "根据评论ID获取点踩其的用户", Description = "根据评论ID获取点踩其的用户")]
    public async Task<IActionResult> GetUsersByComment(int commentId)
    {
        try
        {
            var users = await context.CommentDislikeSet
                .Where(cd => cd.CommentId == commentId)
                .Include(cd => cd.User)
                .Select(cd => cd.UserId)
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "根据用户ID获取其点踩的评论", Description = "根据用户ID获取其点踩的评论")]
    public async Task<IActionResult> GetCommentsByUser(int userId)
    {
        try
        {
            var comments = await context.CommentDislikeSet
                .Where(cd => cd.UserId == userId)
                .Include(cd => cd.Comment)
                .Select(cd => cd.CommentId)
                .ToListAsync();

            return Ok(comments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("{commentId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "点踩评论", Description = "点踩评论")]
    public async Task<IActionResult> DislikeComment(int commentId, int userId)
    {
        var exists = await context.CommentDislikeSet
            .AnyAsync(cd => cd.CommentId == commentId && cd.UserId == userId);

        if (exists)
        {
            return BadRequest($"User has already disliked this comment.");
        }

        var dislike = new CommentDislike
        {
            CommentId = commentId,
            UserId = userId,
            DislikeTime = DateTime.Now
        };

        context.CommentDislikeSet.Add(dislike);
        
        var comment = await context.CommentSet.FindAsync(commentId);
        if (comment != null)
        {
            comment.DislikeCount++;
        }

        await context.SaveChangesAsync();
        return Ok($"Comment with ID: {commentId} has been disliked successfully by user: {userId}.");
    }
        
    
    [HttpDelete("{commentId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "取消点踩", Description = "取消点踩")]
    public async Task<IActionResult> UndislikeComment(int commentId, int userId)
    {
        var dislike = await context.CommentDislikeSet
            .FirstOrDefaultAsync(cd => cd.CommentId == commentId && cd.UserId == userId);

        if (dislike == null)
        {
            return NotFound($"No corresponding dislike found for comment ID: {commentId} and user ID: {userId}.");
        }

        context.CommentDislikeSet.Remove(dislike);
        
        var comment = await context.CommentSet.FindAsync(commentId);
        if (comment != null && comment.DislikeCount > 0)
        {
            comment.DislikeCount--;
        }

        await context.SaveChangesAsync();
        
        return Ok($"Dislike for comment ID: {commentId} by user: {userId} has been successfully removed.");
    }
}