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
                    userId = u.UserId,
                    username = u.UserName,
                    points = u.Points,
                    avatarUrl = u.AvatarUrl,
                    gender = u.Gender,
                    profile = u.Profile,
                    region = u.Region,
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
    public async Task<ActionResult<object>> GetPostsByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var totalCount = await context.PostCollectionSet
                .Where(pc => pc.UserId == userId)
                .CountAsync();
            
            var posts = await (from pc in context.PostCollectionSet
                               join post in context.PostSet on pc.PostId equals post.PostId
                               join userPost in context.UserPostSet on post.PostId equals userPost.PostId
                               join user in context.UserSet on userPost.UserId equals user.UserId
                               where pc.UserId == userId
                               orderby post.PostId descending
                               select new
                               {
                                   postId = post.PostId,
                                   content = post.PostContent,
                                   title = post.PostTitle,
                                   postTime = post.PostTime,
                                   postStatus = post.PostStatus,
                                   stats = new 
                                   {
                                       commentCount = post.CommentCount,  
                                       collectionCount = post.CollectionCount, 
                                       likeCount = post.LikeCount, 
                                       dislikeCount = post.DislikeCount  
                                   },
                                   author = new
                                   {
                                       userId = user.UserId,
                                       username = user.UserName, 
                                       points = user.Points,  
                                       avatarUrl = user.AvatarUrl,  
                                       gender = user.Gender,  
                                       profile = user.Profile,  
                                       region = user.Region 
                                   }
                               })
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            if (posts.Count == 0)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }

            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                list = posts
            });
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
        var user = await context.UserSet.FindAsync(userId);
        if (user == null)
        {
            return NotFound($"No corresponding data found for ID: {userId}");
        }
        
        var post = await context.PostSet.FindAsync(postId);
        if (post == null)
        {
            return NotFound($"No corresponding data found for ID: {postId}");
        }
        
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
        post.CollectionCount++;
        await context.SaveChangesAsync();

        return Ok($"Data with ID: {postId} {userId} has been added successfully.");
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