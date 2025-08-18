using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("POST")]
[SwaggerSchema(Description = "帖子表")]
public class Post
{
    [Key]
    [Column("POST_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema("帖子ID")]
    public int PostId { get; set; }

    [Column("POST_CONTENT")]
    [SwaggerSchema("文本内容")]
    public string? PostContent { get; set; }

    [Column("POST_TITLE")]
    [MaxLength(50)]
    [SwaggerSchema("标题")]
    public string? PostTitle { get; set; }

    [Column("POST_TIME")]
    [SwaggerSchema("发布时间")]
    public DateTime? PostTime { get; set; }

    [Column("POST_STATUS")]
    [MaxLength(20)]
    [SwaggerSchema("发布状态")]
    public string? PostStatus { get; set; }

    [Column("COMMENT_COUNT")]
    [SwaggerSchema("阅读量")]
    public int? CommentCount { get; set; }

    [Column("COLLECTION_COUNT")]
    [SwaggerSchema("收藏量")]
    public int? CollectionCount { get; set; }

    [Column("LIKE_COUNT")]
    [SwaggerSchema("点赞量")]
    public int? LikeCount { get; set; }

    [Column("DISLIKE_COUNT")]
    [SwaggerSchema("点踩量")]
    public int? DislikeCount { get; set; }

}