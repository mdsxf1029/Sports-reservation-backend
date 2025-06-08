using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Sports_reservation_backend.Models.TableModels;

[Table("POST_LIKE")]
[PrimaryKey(nameof(UserId), nameof(PostId))]
[SwaggerSchema("评论点赞表")]
public class PostLike
{
    // 属性定义
    [Column("USER_ID")]
    [ForeignKey("User")]
    [SwaggerSchema(Description = "用户ID")]
    public int UserId { get; set; }
    
    [Column("POST_ID")]
    [ForeignKey("Post")]
    [SwaggerSchema(Description = "帖子ID")]
    public int PostId { get; set; }
    
    // 关系定义
    public User? User { get; set; }
    public Post? Post { get; set; }
}