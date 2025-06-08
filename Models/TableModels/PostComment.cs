using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("POST_COMMENT")]
[PrimaryKey(nameof(PostId),nameof(CommentId))]
[SwaggerSchema("帖子评论表")]
public class PostComment
{
    // 属性定义
    [Column("POST_ID")]
    [ForeignKey("Post")]
    [SwaggerSchema(Description = "帖子ID")]
    public int PostId { get; set; }
    
    [Column("COMMENT_ID")]
    [ForeignKey("Comment")]
    [SwaggerSchema(Description = "评论ID")]
    public int CommentId { get; set; }
    
    // 关系定义
    public Post? Post { get; set; }
    public Comment? Comment { get; set; }   
}
