using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("APPEAL")]
[SwaggerSchema(Description = "申诉表")]
public class Appeal
{
    [Key]
    [Column("APPEAL_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema("申诉ID")]
    public int AppealId { get; set; }

    [Column("VIOLATION_ID")]
    [SwaggerSchema("违约记录ID")]
    public int? ViolationId { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("申诉用户ID")]
    public int? UserId { get; set; }

    [Column("APPEAL_REASON")]
    [SwaggerSchema("申诉理由")]
    public string? AppealReason { get; set; }

    [Column("APPEAL_TIME")]
    [SwaggerSchema("申诉时间")]
    public DateTime? AppealTime { get; set; }

    [Column("APPEAL_STATUS")]
    [StringLength(20)]
    [SwaggerSchema("申诉状态：pending / approved / rejected")]
    public string? AppealStatus { get; set; } = "pending";

    [Column("PROCESS_TIME")]
    [SwaggerSchema("处理时间")]
    public DateTime? ProcessTime { get; set; }

    [Column("PROCESSOR_ID")]
    [SwaggerSchema("处理人ID")]
    public int? ProcessorId { get; set; }

    [Column("REJECT_REASON")]
    [SwaggerSchema("拒绝理由")]
    public string? RejectReason { get; set; }

    [ForeignKey(nameof(ViolationId))]
    public Violation? Violation { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(ProcessorId))]
    public User? Processor { get; set; }
}
