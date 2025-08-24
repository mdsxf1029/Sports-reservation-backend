using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.ResponseModels;
using Sports_reservation_backend.Models.TableModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/post")]
[ApiController]
[SwaggerTag("帖子相关api")]
public class PostController(OracleDbContext context) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "获取所有的帖子", Description = "获取所有的帖子")]
    [SwaggerResponse(200,"获取数据成功")]
    [SwaggerResponse(500,"数据库内部出错")]
    public async Task<ActionResult<object>> GetPost([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var totalCount = await context.PostSet.CountAsync();
            
            var posts = await context.PostSet
                .OrderByDescending(p => p.PostId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = posts
            });
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, $"Database update error: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("public")]
    [SwaggerOperation(Summary = "获取所有公开的帖子", Description = "获取所有公开的帖子")]
    [SwaggerResponse(200,"获取数据成功")]
    [SwaggerResponse(500,"数据库内部出错")]
    public async Task<ActionResult<object>> GetPublicPost([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var totalCount = await context.PostSet.Where(post => post.PostStatus == "public").CountAsync();
            
            var publicPosts = await context.PostSet
                .Where(post => post.PostStatus == "public")
                .OrderByDescending(p => p.PostId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = publicPosts
            });
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, $"Database update error: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取帖子表的数据", Description = "根据主键（ID）获取帖子表的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> GetPostByPk(int id)
    {
        try
        {
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            var userPost = await context.UserPostSet.FindAsync(id);
            if (userPost == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }
            var user = await context.UserSet
                .Where(u => u.UserId == userPost.UserId)
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
            
            return Ok(new {post, user});
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "根据用户ID获取帖子表的数据", Description = "根据用户ID获取帖子表的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> GetPostByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var postIds = await context.UserPostSet.Where(pu => pu.UserId == userId)
                .Select(pu => pu.PostId)
                .ToListAsync();

            if (postIds.Count == 0)
            {
                return NotFound($"No corresponding data found for user ID: {userId}");
            }

            var totalCount = postIds.Count;

            var posts = await context.PostSet
                .Where(p => postIds.Contains(p.PostId))
                .OrderByDescending(p => p.PostId) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = posts
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("user/{userId:int}")]
    [SwaggerOperation(Summary = "添加帖子表的数据", Description = "添加帖子表的数据")]
    [SwaggerResponse(201, "添加数据项成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> PostPost(int userId, [FromBody]Post post)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            post.LikeCount = 0;
            post.DislikeCount = 0;
            post.CommentCount = 0;
            post.CollectionCount = 0;
            post.PostTime = DateTime.Now;
            post.PostStatus = "public";

            context.PostSet.Add(post);

            await context.SaveChangesAsync();

            var userPost = new UserPost
            {
                UserId = userId,
                PostId = post.PostId
            };
            context.UserPostSet.Add(userPost);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostPost), new { id = post.PostId }, post);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除帖子表的数据", Description = "根据主键（ID）删除帖子表的数据")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> DeletePostByPk(int id)
    {
        try
        {
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }
            
            var userPost = context.UserPostSet.Where(up => up.PostId == id);
            
            context.UserPostSet.RemoveRange(userPost);
            context.PostSet.Remove(post);
            
            await context.SaveChangesAsync();
            
            return Ok($"Data with ID: {id} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "更新帖子的数据", Description = "更新帖子的数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] Post post)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id != post.PostId)
        {
            return BadRequest("ID mismatch");
        }
        
        var existingPost = await context.PostSet.FindAsync(id);
        if (existingPost == null)
        {
            return NotFound($"No corresponding data found for ID: {id}");
        }
        
        existingPost.PostTitle = post.PostTitle;
        existingPost.PostContent = post.PostContent;
        
        try
        {
            context.Entry(post).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.PostSet.Any(e => e.PostId == id))
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return Ok($"Data with ID: {id} has been updated successfully.");
    }
    
}