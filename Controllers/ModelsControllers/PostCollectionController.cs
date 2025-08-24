using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.ResponseModels;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/post-collection")]
[ApiController]
[SwaggerTag("帖子收藏相关api")]
public class PostCollectionController(OracleDbContext context) : ControllerBase
{
    [HttpGet("post/{postId:int}/users")]
    [SwaggerOperation(Summary = "根据帖子ID获取收藏其的用户", Description = "根据帖子ID获取收藏其的用户")]
    public async Task<ActionResult<object>> GetUsersByPost(int postId)
    {
        try
        {
            var userIds = await context.PostCollectionSet
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
            
            return Ok(new { count = users.Count , data = users });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("user/{userId:int}/posts")]
    [SwaggerOperation(Summary = "根据用户ID获取其收藏的帖子", Description = "根据用户ID获取其收藏的帖子")]
    public async Task<ActionResult<object>> GetPostsByUser(int userId)
    {
        try
        {
            var posts = await context.PostCollectionSet
                .Where(pc => pc.UserId == userId)
                .Include(pl => pl.Post)
                .Select(pl => pl.Post)
                .ToListAsync();

            if (posts.Count == 0)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }

            return Ok(new { count = posts.Count , data = posts});
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("{userId:int}-{postId:int}")]
    [SwaggerOperation(Summary = "收藏帖子", Description = "收藏帖子")]
    public async Task<IActionResult> AddCollection(int userId, int postId)
    {
        var exists = await context.PostCollectionSet
            .AnyAsync(pc => pc.UserId == userId && pc.PostId == postId);

        if (exists)
        {
            return BadRequest("This post is already collected by the user.");
        }
        
        var collection = new PostCollection
        {
            UserId = userId,
            PostId = postId,
            CollectedTime = DateTime.Now
        };

        context.PostCollectionSet.Add(collection);
        
        var post = await context.PostSet.FindAsync(postId);
        if (post != null)
        {
            post.LikeCount++;
        }
        
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(AddCollection), new { userId, postId }, collection);
    }
    
    [HttpDelete("{userId:int}-{postId:int}")]
    [SwaggerOperation(Summary = "取消收藏", Description = "取消收藏")]
    public async Task<IActionResult> RemoveCollection(int userId, int postId)
    {
        try
        {
            var collection = await context.PostCollectionSet
                .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.PostId == postId);

            if (collection == null)
            {
                return NotFound("Collection not found.");
            }

            context.PostCollectionSet.Remove(collection);

            var post = await context.PostSet.FindAsync(postId);
            if (post != null && post.LikeCount > 0)
            {
                post.LikeCount--;
            }
            
            await context.SaveChangesAsync();

            return Ok("Collection removed successfully.");
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}