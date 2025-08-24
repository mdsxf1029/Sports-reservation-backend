using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.ResponseModels;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/post-like")]
[ApiController]
[SwaggerTag("帖子点赞相关api")]
public class PostLikeController(OracleDbContext context) : ControllerBase
{
    [HttpGet("post/{postId:int}")]
    [SwaggerOperation(Summary = "根据帖子ID获取点赞其的用户", Description = "根据帖子ID获取点赞其的用户")]
    public async Task<ActionResult<object>> GetUsersByPost(int postId)
    {
        try
        {
            var userIds = await context.PostLikeSet
                .Where(p => p.PostId == postId)
                .Select(pl => pl.UserId)
                .ToListAsync();

            if (userIds.Count == 0)
            {
                return NotFound($"No corresponding data found for ID: {postId}");
            }

            var users = await context.UserSet
                .Where(u => userIds.Contains(u.UserId))
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
                .ToListAsync();

            return Ok(new { count = users.Count, data = users });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "根据用户ID获取其点赞的帖子", Description = "根据用户ID获取其点赞的帖子")]
    public async Task<ActionResult<object>> GetPostsByUser(int userId)
    {
        try
        {
            var posts = await context.PostLikeSet
                .Where(pl => pl.UserId == userId)
                .Include(pl => pl.Post)
                .Select(pl => pl.Post)
                .ToListAsync();

            return Ok(new { count = posts.Count, data = posts });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("{postId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "点赞帖子", Description = "点赞帖子")]
    public async Task<IActionResult> LikePost(int postId, int userId)
    {
        var exists = await context.PostLikeSet
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);

        if (exists)
        {
            return BadRequest($"Data with ID: {postId} {userId} already exists.");
        }

        var like = new PostLike
        {
            PostId = postId,
            UserId = userId,
            LikeTime = DateTime.Now
        };

        context.PostLikeSet.Add(like);
        
        var post = await context.PostSet.FindAsync(postId);
        if (post != null)
        {
            post.LikeCount++;
        }

        await context.SaveChangesAsync();
        return Ok($"Data with ID: {postId} {userId} has been added successfully.");
    }
    
    [HttpDelete("{postId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "取消点赞", Description = "取消点赞")]
    public async Task<IActionResult> UnlikePost(int postId, int userId)
    {
        var like = await context.PostLikeSet
            .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);

        if (like == null)
        {
            return NotFound($"No corresponding data found for ID: {postId} {userId}.");
        }

        context.PostLikeSet.Remove(like);
        
        var post = await context.PostSet.FindAsync(postId);
        if (post != null && post.LikeCount > 0)
        {
            post.LikeCount--;
        }

        await context.SaveChangesAsync();
        
        return Ok($"Data with ID: {postId} {userId} has been deleted successfully.");
    }
}