using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("USERS_COLLECTION")]
[SwaggerSchema("用户收藏")]
public class PostCollection
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
    
    [Required]
    [Column("COLLECTED_TIME")]
    [SwaggerSchema(Description = "收藏时间")]
    public DateTime CollectedTime { get; set; }
    
    // 关系定义
    public User? User { get; set; }
    public Post? Post { get; set; }
}