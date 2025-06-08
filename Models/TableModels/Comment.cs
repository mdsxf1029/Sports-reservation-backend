using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("COMMENTS")]
[SwaggerSchema(Description = "评论表")]
public class Comment
{
    // 属性定义
    [Key]
    [Column("COMMENTS_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema(Description = "评论ID")]
    public int CommentId { get; set; }
    
    [Required]
    [Column("COMMENTS_COMMENT")]
    [SwaggerSchema(Description = "评论内容")]
    [MaxLength]
    public string CommentContent { get; set; } =string.Empty;
    
    [Required]
    [Column("COMMENTS_PUBLISH_TIME")]
    [SwaggerSchema(Description = "评论发布时间")]
    public DateTime CommentPublishTime { get; set; }
    
    [Required]
    [Column("COMMENTS_PUBLISH_STATUS")]
    [SwaggerSchema(Description = "发布状态")]
    [MaxLength(20)]
    public string CommentPublishStatus { get; set; } = string.Empty;
    
}