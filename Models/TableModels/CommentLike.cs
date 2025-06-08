using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("COMMENT_LIKE")]
[PrimaryKey(nameof(UserId), nameof(CommentId))]
[SwaggerSchema("评论点赞表")]
public class CommentLike
{
    // 属性定义
    [Column("USER_ID")]
    [ForeignKey("User")]
    [SwaggerSchema(Description = "用户ID")]
    public int UserId { get; set; }
    
    [Column("COMMENT_ID")]
    [ForeignKey("Comment")]
    [SwaggerSchema(Description = "评论ID")]
    public int CommentId { get; set; }
    
    // 关系定义
    public User? User { get; set; }
    public Comment? Comment { get; set; }
}