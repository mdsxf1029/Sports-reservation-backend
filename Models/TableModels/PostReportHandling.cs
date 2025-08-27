using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("POST_REPORT_HANDLING")]
[SwaggerSchema(Description = "帖子举报审核表")]
public class PostReportHandling
{
    [Key]
    [Column("HANDLE_ID")]
    [SwaggerSchema("处理ID")]
    public int? HandleId { get; set; }
    
    [Column("MANAGER_ID")]
    [SwaggerSchema("管理员ID")]
    public int? ManagerId { get; set; }
    
    [Column("REPORT_ID")]
    [SwaggerSchema("举报ID")]
    public int ReportId { get; set; }

    [Column("MANAGE_TIME")]
    [SwaggerSchema("处理时间")]
    public DateTime? ManageTime { get; set; }

    [Column("MANAGE_REASON")]
    [SwaggerSchema("处理原因")]
    public string? ManageReason { get; set; }

    [ForeignKey(nameof(ManagerId))]
    public User? Manager { get; set; }

    [ForeignKey(nameof(ReportId))]
    public PostReport? PostReport { get; set; }
}
