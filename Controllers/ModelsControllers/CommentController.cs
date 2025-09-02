using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Sports_reservation_backend.Data;
using Sports_reservation_backend.Models.ResponseModels;
using Sports_reservation_backend.Models.TableModels;

namespace Sports_reservation_backend.Controllers.ModelsControllers;

[Route("api/comment")]
[ApiController]
[SwaggerTag("帖子评论相关api")]
public class CommentController(OracleDbContext context) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "获取评论数据", Description = "分页获取所有评论数据")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(500, "数据库内部出错")]
    public async Task<ActionResult<object>> GetComment([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var totalCount = await context.CommentSet.CountAsync();

            var comments = await context.CommentSet
                .OrderByDescending(c => c.CommentId) // 按照 CommentId 排序
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = comments
            });
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
    public async Task<ActionResult<object>> GetCommentByPk(int commentId)
    {
        try
        {
            var comment = await context.CommentSet.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound($"No corresponding data found for ID: {commentId}");
            }

            var userComment = await context.UserCommentSet.FindAsync(commentId);
            if (userComment == null)
            {
                return NotFound($"No corresponding data found for ID: {commentId}");
            }
            var user = await context.UserSet
                .Where(u => u.UserId == userComment.UserId)
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
                .FirstOrDefaultAsync();
            
            return Ok(new {comment, user});
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
    public async Task<ActionResult<object>> GetCommentByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var commentIds = await context.UserCommentSet.Where(pu => pu.UserId == userId)
                .Select(pu => pu.CommentId)
                .ToListAsync();
        
            if (commentIds.Count == 0)
            {
                return NotFound($"No corresponding data found for ID: {userId}");
            }
        
            var totalCount = commentIds.Count;
            
            var comments = await context.CommentSet
                .Where(c => commentIds.Contains(c.CommentId))
                .OrderByDescending(c => c.CommentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = comments
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("post/{postId:int}")]
    [SwaggerOperation(Summary = "根据PostId获取评论数据", Description = "分页获取某个帖子下的所有评论")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> GetCommentByPostId(int postId)
    {
        try
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                var comments = await (from pc in context.PostCommentSet
                        join c in context.CommentSet on pc.CommentId equals c.CommentId
                        join uc in context.UserCommentSet on c.CommentId equals uc.CommentId
                        join u in context.UserSet on uc.UserId equals u.UserId
                        where pc.PostId == postId
                        orderby pc.CommentId descending
                        select new
                        {
                            commentId = c.CommentId,
                            content = c.CommentContent,
                            publishTime = c.CommentTime,
                            author = new
                            {
                                userId = u.UserId,
                                username = u.UserName,
                                avatar = u.AvatarUrl
                            },
                            likeCount = c.LikeCount,
                            dislikeCount = c.DislikeCount,
                            isLiked = false,
                            isDisliked = false
                        })
                        .ToListAsync();
                
                return Ok(new { comments });
            }
            else
            {
                var comments = await (from pc in context.PostCommentSet
                        // 和评论做连接
                        join c in context.CommentSet on pc.CommentId equals c.CommentId
                        // 和用户左连接
                        join uc in context.UserCommentSet on c.CommentId equals uc.CommentId
                        join u in context.UserSet on uc.UserId equals u.UserId
                        where pc.PostId == postId
                        // 和用户点踩做连接
                        join like in context.CommentLikeSet.Where(l => l.UserId == userId) on c.CommentId equals like.CommentId into likeGroup
                        from userLike in likeGroup.DefaultIfEmpty()
                        // 和用户点踩做连接
                        join dislike in context.CommentDislikeSet.Where(d => d.UserId == userId) on c.CommentId equals dislike.CommentId into dislikeGroup
                        from userDislike in dislikeGroup.DefaultIfEmpty()
                        orderby pc.CommentId descending
                        select new
                        {
                            commentId = c.CommentId,
                            content = c.CommentContent,
                            publishTime = c.CommentTime,
                            author = new
                            {
                                userId = u.UserId,
                                username = u.UserName,
                                avatar = u.AvatarUrl
                            },
                            likeCount = c.LikeCount,
                            dislikeCount = c.DislikeCount,
                            isLiked = userLike != null,
                            isDisliked = userDislike != null
                        })
                        .ToListAsync();

                return Ok(new { comments });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("comment/{commentId:int}")]
    [SwaggerOperation(Summary = "根据CommentId获取评论的回复数据", Description = "分页获取某个评论下的所有回复")]
    [SwaggerResponse(200, "获取数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<ActionResult<object>> GetRepliesByComment(int commentId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        
        pageSize = pageSize < 1 ? 10 : pageSize;
        
        try
        {
            var totalCount = await context.CommentReplySet.Where(cr => cr.CommentId == commentId).CountAsync();

            var replies = await context.CommentReplySet
                .Where(cr => cr.CommentId == commentId)
                .Include(cr => cr.Reply)
                .OrderByDescending(cr => cr.ReplyId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cr => cr.Reply)
                .ToListAsync();

            if (replies.Count == 0)
            {
                return NotFound($"No replies found for CommentId: {commentId}");
            }

            return Ok(new
            {
                page, pageSize, totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = replies
            });
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
        
        try
        {
            comment.CommentStatus = "public";
            comment.CommentTime = DateTime.Now;
            comment.LikeCount = 0;
            comment.DislikeCount = 0;

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
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
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
        
        reply.CommentStatus = "public";
        reply.CommentTime = DateTime.Now;
        reply.LikeCount = 0;
        reply.DislikeCount = 0;
        
        context.CommentSet.Add(reply);
        
        await context.SaveChangesAsync();

        var commentReply = new CommentReply
        {
            CommentId = commentId,
            ReplyId = reply.CommentId
        };
        
        context.CommentReplySet.Add(commentReply);
        
        var userComment = new UserComment
        {
            UserId = userId,
            CommentId = reply.CommentId
        };
        
        context.UserCommentSet.Add(userComment);
        
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(PostCommentComment), new { id = comment.CommentId }, comment);
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "根据主键（ID）删除评论表的数据", Description = "根据主键（ID）删除评论表的数据")]
    [SwaggerResponse(200, "删除数据成功")]
    [SwaggerResponse(404, "未找到对应数据")]
    [SwaggerResponse(500, "服务器内部错误")]
    public async Task<IActionResult> DeleteCommentByPk(int id)
    {
        try
        {
            var comment = await context.CommentSet.FindAsync(id);
            if (comment == null)
            {
                return NotFound($"No corresponding data found for ID: {id}");
            }

            // 删除与该评论相关的帖子评论记录
            var commentPost = context.PostCommentSet.Where(pc => pc.CommentId == id);
            if (commentPost.Any())
            {
                context.PostCommentSet.RemoveRange(commentPost);
                // 同步修改对应帖子的评论数
                var postId = commentPost.Select(pc => pc.PostId).FirstOrDefault();
                var post = await context.PostSet.FindAsync(postId);
                if (post != null)
                {
                    post.CommentCount = post.CommentCount > 0 ? post.CommentCount - 1 : 0;
                    context.PostSet.Update(post);
                }
            }
            
            await DeleteCommentRepliesRecursive(id);
            
            await context.SaveChangesAsync();
            
            return Ok($"Data with ID: {id} has been deleted successfully.");
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
    
    private async Task DeleteCommentRepliesRecursive(int parentId)
    {
        // 查找当前评论的所有回复
        var replies = await context.CommentReplySet.Where(cr => cr.CommentId == parentId).ToListAsync();

        // 递归地先删除所有子回复，确保从最底层开始删除
        foreach (var reply in replies)
        {
            await DeleteCommentRepliesRecursive(reply.ReplyId);
        }

        // 删除当前评论的所有回复
        context.CommentReplySet.RemoveRange(replies);

        // 删除当前评论的所有举报
        var commentReports = context.CommentReportSet.Where(cp => cp.ReportedCommentId == parentId);
        foreach (var commentReport in commentReports)
        {
            var reportHandling = context.CommentReportHandlingSet.Where(rh => rh.ReportId == commentReport.ReportId);
            context.CommentReportHandlingSet.RemoveRange(reportHandling);
        }
        context.CommentReportSet.RemoveRange(commentReports);
        
        // 删除与当前评论及其所有子回复相关的点赞、踩和用户评论记录
        var commentLikes = context.CommentLikeSet.Where(cl => cl.CommentId == parentId);
        context.CommentLikeSet.RemoveRange(commentLikes);

        var commentDislikes = context.CommentDislikeSet.Where(cd => cd.CommentId == parentId);
        context.CommentDislikeSet.RemoveRange(commentDislikes);

        var userComments = context.UserCommentSet.Where(uc => uc.CommentId == parentId);
        context.UserCommentSet.RemoveRange(userComments);

        // 删除与当前评论 ID 对应的 CommentSet 记录
        var comment = await context.CommentSet.FindAsync(parentId);
        if (comment != null)
        {
            context.CommentSet.Remove(comment);
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
        
        var existingComment = await context.CommentSet.FindAsync(id);

        if (existingComment == null)
        {
            return NotFound($"No corresponding data found for ID: {id}");
        }
        existingComment.CommentContent = comment.CommentContent;
        
        try
        {
            context.Entry(existingComment).State = EntityState.Modified;
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