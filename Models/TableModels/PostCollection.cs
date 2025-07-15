using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Sports_reservation_backend.Models.TableModels;

[PrimaryKey(nameof(UserId), nameof(PostId))]
[Table("POST_COLLECTION")]
[SwaggerSchema(Description = "用户收藏记录表")]
public class PostCollection
{
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Column("POST_ID")]
    [SwaggerSchema("帖子ID")]
    public int PostId { get; set; }

    [Required]
    [Column("COLLECT_TIME")]
    [SwaggerSchema("收藏时间")]
    public DateTime CollectTime { get; set; }

    [ForeignKey(nameof(PostId))]
    public Post? Post { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
