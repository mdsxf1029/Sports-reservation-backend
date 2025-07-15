using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("POST_COMMENT")]
[SwaggerSchema(Description = "帖子评论记录")]
public class PostComment
{
    [Column("POST_ID")]
    [SwaggerSchema("帖子ID")]
    public int? PostId { get; set; }

    [Key]
    [Column("COMMENT_ID")]
    [SwaggerSchema("评论ID")]
    public int CommentId { get; set; }

    [ForeignKey(nameof(PostId))]
    public Post? Post { get; set; }

    [ForeignKey(nameof(CommentId))]
    public Comment? Comment { get; set; }
}
