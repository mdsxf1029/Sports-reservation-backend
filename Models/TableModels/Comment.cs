using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels
{
    [Table("\"COMMENT\"")]
    [SwaggerSchema(Description = "评论表")]
    public class Comment
    {
        [Key]
        [Column("COMMENT_ID")]
        [SwaggerSchema(Description = "评论ID")]
        public int CommentId { get; set; }

        [Required]
        [Column("COMMENT_CONTENT")]
        [SwaggerSchema(Description = "评论内容")]
        public string CommentContent { get; set; } = string.Empty;

        [Required]
        [Column("COMMENT_TIME")]
        [SwaggerSchema(Description = "发布时间")]
        public DateTime CommentTime { get; set; }

        [Required]
        [Column("COMMENT_STATUS")]
        [SwaggerSchema(Description = "发布状态")]
        [MaxLength(20)]
        public string CommentStatus { get; set; } = string.Empty;

        [Required]
        [Column("LIKE_COUNT")]
        [SwaggerSchema(Description = "点赞数")]
        public int LikeCount { get; set; }

        [Required]
        [Column("DISLIKE_COUNT")]
        [SwaggerSchema(Description = "点踩数")]
        public int DislikeCount { get; set; }
    }
}