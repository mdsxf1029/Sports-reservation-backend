using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("COMMENT_REPLY")]
[SwaggerSchema(Description = "评论回复记录表")]
public class CommentReply
{
    [Column("COMMENT_ID")]
    [SwaggerSchema("评论ID")]
    public int? CommentId { get; set; }

    [Key]
    [Column("REPLY_ID")]
    [SwaggerSchema("回复评论ID")]
    public int ReplyId { get; set; }

    [ForeignKey(nameof(CommentId))]
    public Comment? Comment { get; set; }

    [ForeignKey(nameof(ReplyId))]
    public Comment? Reply { get; set; }
}
