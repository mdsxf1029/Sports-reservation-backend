using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/post-dislike")]
[ApiController]
[SwaggerTag("帖子点睬相关api")]
public class PostDislikeController(OracleDbContext context) : ControllerBase
{
    [HttpGet("post/{postId:int}")]
    [SwaggerOperation(Summary = "根据帖子ID获取点踩其的用户", Description = "根据帖子ID获取点踩其的用户")]
    public async Task<IActionResult> GetUsersByPost(int postId)
    {
        try
        {
            var users = await context.PostDislikeSet
                .Where(pd => pd.PostId == postId)
                .Include(pd => pd.User)
                .Select(pd => pd.User)
                .ToListAsync();

            return Ok(new{ count = users.Count, data = users });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "根据用户ID获取其点踩的帖子", Description = "根据用户ID获取其点踩的帖子")]
    public async Task<IActionResult> GetPostsByUser(int userId)
    {
        try
        {
            var posts = await context.PostDislikeSet
                .Where(pd => pd.UserId == userId)
                .Include(pd => pd.Post)
                .Select(pd => pd.Post)
                .ToListAsync();

            return Ok(new {count = posts.Count, data = posts });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("{postId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "点踩帖子", Description = "点踩帖子")]
    public async Task<ActionResult<object>> DislikePost(int postId, int userId)
    {
        var exists = await context.PostDislikeSet
            .AnyAsync(pd => pd.PostId == postId && pd.UserId == userId);

        if (exists)
        {
            return BadRequest($"Data with ID: {postId} {userId} already exists.");
        }

        var dislike = new PostDislike
        {
            PostId = postId,
            UserId = userId,
            DislikeTime = DateTime.Now
        };

        context.PostDislikeSet.Add(dislike);
        
        var post = await context.PostSet.FindAsync(postId);
        if (post != null)
        {
            post.DislikeCount++;
        }

        await context.SaveChangesAsync();
        return Ok($"Data with ID: {postId} {userId} has been added successfully.");
    }
    
    [HttpDelete("{postId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "取消点踩", Description = "取消点踩")]
    public async Task<ActionResult<object>> UndislikePost(int postId, int userId)
    {
        var dislike = await context.PostDislikeSet
            .FirstOrDefaultAsync(pd => pd.PostId == postId && pd.UserId == userId);

        if (dislike == null)
        {
            return NotFound($"No corresponding data found for ID: {postId} {userId}.");
        }
        
        context.PostDislikeSet.Remove(dislike);
        
        var post = await context.PostSet.FindAsync(postId);
        if (post != null && post.DislikeCount > 0)
        {
            post.DislikeCount--;
        }

        await context.SaveChangesAsync();
        return Ok($"Data with ID: {postId} {userId} has been deleted successfully.");
    }
}