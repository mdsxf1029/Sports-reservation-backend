using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("COMMENT_REPORT")]
[SwaggerSchema(Description = "评论举报表")]
public class CommentReport
{
    [Key]
    [Column("REPORT_ID")]
    [SwaggerSchema("举报ID")]
    public int ReportId { get; set; }

    [Column("REPORTER_ID")]
    [SwaggerSchema("举报人ID")]
    public int? ReporterId { get; set; }

    [Column("REPORTED_USER_ID")]
    [SwaggerSchema("被举报用户ID")]
    public int? ReportedUserId { get; set; }

    [Column("REPORTED_COMMENT_ID")]
    [SwaggerSchema("被举报评论ID")]
    public int? ReportedCommentId { get; set; }

    [Column("REPORT_REASON")]
    [SwaggerSchema("举报原因")]
    public string? ReportReason { get; set; }

    [Column("REPORT_TIME")]
    [SwaggerSchema("举报时间")]
    public DateTime? ReportTime { get; set; }

    [Column("REPORT_STATUS")]
    [MaxLength(20)]
    [SwaggerSchema("举报状态：checking / accepted / rejected")]
    public string? ReportStatus { get; set; }

    [ForeignKey(nameof(ReporterId))]
    public User? Reporter { get; set; }

    [ForeignKey(nameof(ReportedUserId))]
    public User? ReportedUser { get; set; }

    [ForeignKey(nameof(ReportedCommentId))]
    public Comment? ReportedComment { get; set; }
}
