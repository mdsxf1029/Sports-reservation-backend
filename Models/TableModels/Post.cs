using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("POST")]
public class Post
{
    [Key]
    [Column("POST_ID")]
    [SwaggerSchema(Description = "帖子ID")]
    public int PostId { get; set; }
    
    [Required]
    [Column("POST_TITLE")]
    [MaxLength(50)]
    [SwaggerSchema(Description = "帖子标题")]
    public string PostTitle { get; set; }=string.Empty;
    
    [Required]
    [Column("POST_CONTENT")]
    [MaxLength]
    [SwaggerSchema(Description = "帖子内容")]
    public string PostContent { get; set; } = string.Empty;
    
    [Required]
    [Column("POST_READING_VOLUME")]
    [SwaggerSchema(Description = "帖子浏览量")]
    public int PostReadingVolume { get; set; }
    
    [Required]
    [Column("POST_PUBLISH_STATUS")]
    [MaxLength(20)]
    [SwaggerSchema(Description = "发布状态")]
    public string PostPublishStatus { get; set; } = string.Empty;
    
}