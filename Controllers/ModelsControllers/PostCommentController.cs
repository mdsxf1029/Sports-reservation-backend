using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/post-comment")]
[ApiController]
[SwaggerTag("帖子评论相关api")]
public class PostCommentController(OracleDbContext context) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation]
    [SwaggerResponse(200,"获取数据成功")]
    [SwaggerResponse(500,"数据库内部出错")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetComment()
    {
        try
        {
            return Ok(await context.CommentSet.ToListAsync());
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
    
    [HttpGet("{commentId:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）获取评论的数据", Description = "根据主键（ID）获取评论的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<PostComment>> GetCommentByPk(int commentId)
    {
        try
        {
            var comment = await context.CommentSet.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound($"No corresponding data found for ID: {commentId}");
            }
            return Ok(comment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "根据用户ID获取评论的数据", Description = "根据用户ID获取评论的数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentByUser(int userId)
    {
        var commentIds = await context.UserPostSet.Where(pu => pu.UserId == userId)
            .Select(pu => pu.PostId)
            .ToListAsync();
        if (commentIds.Count == 0)
        {
            return NotFound($"No corresponding data found for ID: {userId}");
        }
        try
        {
            var comments = await context.CommentSet.Where(p => commentIds.Contains(p.CommentId))
                .ToListAsync();
            return Ok(comments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("post/{postId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "添加评论表的数据", Description = "添加评论表的数据")]
    [SwaggerResponse(201, "添加数据项成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> PostPostComment(int postId, int userId, [FromBody]Comment comment)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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
        
        context.CommentSet.Add(comment);
        post.CommentCount += 1;
        
        await context.SaveChangesAsync();

        var postComment = new PostComment
        {
            PostId = postId,
            CommentId = comment.CommentId
        };
        
        context.PostCommentSet.Add(postComment);
        
        var userComment = new UserComment
        {
            UserId = userId,
            CommentId = comment.CommentId
        };
        
        context.UserCommentSet.Add(userComment);
        
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(PostPostComment), new { id = comment.CommentId }, comment);
    }
    
    [HttpPost("comment/{commentId:int}-{userId:int}")]
    [SwaggerOperation(Summary = "添加评论表的数据", Description = "添加评论表的数据")]
    [SwaggerResponse(201, "添加数据项成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> PostCommentComment(int commentId, int userId, [FromBody]Comment reply)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var user = await context.UserSet.FindAsync(userId);
        if (user == null)
        {
            return NotFound($"No corresponding data found for ID: {userId}");
        }
        
        var comment = await context.CommentSet.FindAsync(commentId);
        if (comment == null)
        {
            return NotFound($"No corresponding data found for ID: {commentId}");
        }
        
        context.CommentSet.Add(reply);
        
        await context.SaveChangesAsync();

        var commentReply = new CommentReply
        {
            CommentId = commentId,
            ReplyId = reply.CommentId
        };
        
        context.CommentReplySet.Add(commentReply);
        
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(PostCommentComment), new { id = comment.CommentId }, comment);
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除评论表的数据", Description = "根据主键（ID）删除评论表的数据")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<Comment>> DeleteCommentByPk(int id)
    {
        try
        {
            var comment = await context.CommentSet.FindAsync(id);
            if (comment == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }
            
            var postComment = context.PostCommentSet.Where(pc => pc.CommentId == id);
            
            context.PostCommentSet.RemoveRange(postComment);
            
            var commentReply = context.CommentReplySet.Where(pc => pc.CommentId == id || pc.ReplyId == id);
            
            context.CommentReplySet.RemoveRange(commentReply);
            
            context.CommentSet.Remove(comment);
            
            await context.SaveChangesAsync();
            
            return Ok($"Data with ID: {id} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "更新评论的数据", Description = "更新评论的数据")]
    [SwaggerResponse(200, "更新数据成功")]
    [SwaggerResponse(400, "请求无效")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> UpdateComment(int id, [FromBody]Comment comment)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id != comment.CommentId)
        {
            return BadRequest("ID mismatch");
        }
        
        context.Entry(comment).State = EntityState.Modified;
        
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.CommentSet.Any(e => e.CommentId == id))
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