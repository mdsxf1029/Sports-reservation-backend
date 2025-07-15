using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Sports_reservation_backend.Models.TableModels;

[PrimaryKey(nameof(UserId), nameof(CommentId))]
[Table("COMMENT_DISLIKE")]
[SwaggerSchema(Description = "用户评论点踩赞记录表")]
public class CommentDislike
{
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Column("COMMENT_ID")]
    [SwaggerSchema("评论ID")]
    public int CommentId { get; set; }

    [Required]
    [Column("DISLIKE_TIME")]
    [SwaggerSchema("点踩时间")]
    public DateTime DislikeTime { get; set; }

    [ForeignKey(nameof(CommentId))]
    public Comment? Comment { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
