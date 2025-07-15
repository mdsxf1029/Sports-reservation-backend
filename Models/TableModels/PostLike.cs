using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Sports_reservation_backend.Models.TableModels;

[PrimaryKey(nameof(UserId), nameof(PostId))]
[Table("POST_LIKE")]
[SwaggerSchema(Description = "用户点赞记录表")]
public class PostLike
{
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Column("POST_ID")]
    [SwaggerSchema("帖子ID")]
    public int PostId { get; set; }

    [Required]
    [Column("LIKE_TIME")]
    [SwaggerSchema("点赞时间")]
    public DateTime LikeTime { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(PostId))]
    public Post? Post { get; set; }
}
