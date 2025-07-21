using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("VIOLATION")]
[SwaggerSchema(Description = "违约记录表")]
public class Violation
{
    [Key]
    [Column("VIOLATION_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema("违约ID")]
    public int ViolationId { get; set; }

    [Column("APPOINTMENT_ID")]
    [SwaggerSchema("预约ID")]
    public int? AppointmentId { get; set; }

    [Column("VIOLATION_REASON")]
    [SwaggerSchema("违约原因")]
    public string? ViolationReason { get; set; }

    [Column("VIOLATION_TIME")]
    [SwaggerSchema("违约时间")]
    public DateTime? ViolationTime { get; set; }

    [Column("VIOLATION_PENALTY")]
    [SwaggerSchema("处罚措施")]
    public string? ViolationPenalty { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment? Appointment { get; set; }
}
