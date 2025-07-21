using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("USER_POST")]
[SwaggerSchema(Description = "用户发帖记录表")]
public class UserPost
{
    [Key]
    [Column("POST_ID")]
    [SwaggerSchema("帖子ID")]
    public int PostId { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int? UserId { get; set; }

    [ForeignKey(nameof(PostId))]
    public Post? Post { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
