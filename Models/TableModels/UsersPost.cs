using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("USERS_POST")]
[PrimaryKey(nameof(UserId),nameof(PostId))]
[SwaggerSchema("用户发帖表")]
public class UsersPost
{
    // 数据定义
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