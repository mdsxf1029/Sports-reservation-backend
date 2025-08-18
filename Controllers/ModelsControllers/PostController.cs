using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_reservation_backend.Data;
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
    public async Task<ActionResult<IEnumerable<Post>>> GetPost()
    {
        try
        {
            return Ok(await context.PostSet.ToListAsync());
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
    public async Task<ActionResult<IEnumerable<Post>>> GetPublicPost()
    {
        try
        {
            var publicposts = await context.PostSet
                .Where(post => post.PostStatus == "public").ToListAsync();
            return Ok(publicposts);
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
    public async Task<ActionResult<Post>> GetPostByPk(int id)
    {
        try
        {
            var post = await context.PostSet.FindAsync(id);
            if (post == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }
            return Ok(post);
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
    public async Task<ActionResult<IEnumerable<Post>>> GetPostByUser(int userId)
    {
        var postIds = await context.UserPostSet.Where(pu => pu.UserId == userId)
            .Select(pu => pu.PostId)
            .ToListAsync();
        if (postIds.Count == 0)
        {
            return NotFound($"No corresponding data found for ID: {userId}");
        }
        try
        {
            var posts = await context.PostSet.Where(p => postIds.Contains(p.PostId))
                .ToListAsync();
            return Ok(posts);
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
    
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除帖子表的数据", Description = "根据主键（ID）删除帖子表的数据")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Post>> DeletePostByPk(int id)
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
        context.Entry(post).State = EntityState.Modified;
        try
        {
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