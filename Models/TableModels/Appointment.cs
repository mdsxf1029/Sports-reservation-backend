using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("APPOINTMENT")]
[SwaggerSchema(Description = "预约表")]
public class Appointment
{
    [Key]
    [Column("APPOINTMENT_ID")]
    [SwaggerSchema("预约ID")]
    public int AppointmentId { get; set; }

    [Column("APPOINTMENT_STATUS")]
    [StringLength(20)]
    [SwaggerSchema("预约状态，upcoming、ongoing、canceled、overtime、completed")]
    public string? AppointmentStatus { get; set; }

    [Column("APPLY_TIME")]
    [SwaggerSchema("申请时间")]
    public DateTime? ApplyTime { get; set; }

    [Column("FINISH_TIME")]
    [SwaggerSchema("实际结束时间")]
    public DateTime? FinishTime { get; set; }

    [Column("BEGIN_TIME")]
    [SwaggerSchema("预约时段起点")]
    public DateTime? BeginTime { get; set; }

    [Column("END_TIME")]
    [SwaggerSchema("预约结束时间")]
    public DateTime? EndTime { get; set; }
}
