using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("USER_COMMENT")]
[SwaggerSchema(Description = "用户评论记录表")]
public class UserComment
{
    [Key]
    [Column("COMMENT_ID")]
    [SwaggerSchema("评论ID")]
    public int CommentId { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(CommentId))]
    public Comment? Comment { get; set; }
}
