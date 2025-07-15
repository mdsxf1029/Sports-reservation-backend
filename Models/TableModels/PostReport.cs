using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("POST_REPORT")]
[SwaggerSchema(Description = "帖子举报表")]
public class PostReport
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

    [Column("REPORTED_POST_ID")]
    [SwaggerSchema("被举报帖子ID")]
    public int? ReportedPostId { get; set; }

    [Column("REPORT_REASON")]
    [SwaggerSchema("举报原因")]
    public string? ReportReason { get; set; }

    [Column("REPORT_TIME")]
    [SwaggerSchema("举报时间")]
    public DateTime? ReportTime { get; set; }

    [Column("REPORT_STATUS")]
    [SwaggerSchema("举报状态：checking / accepted / rejected")]
    [MaxLength(20)]
    public string? ReportStatus { get; set; }


    [ForeignKey(nameof(ReporterId))]
    public User? Reporter { get; set; }

    [ForeignKey(nameof(ReportedUserId))]
    public User? ReportedUser { get; set; }

    [ForeignKey(nameof(ReportedPostId))]
    public Post? ReportedPost { get; set; }
}
