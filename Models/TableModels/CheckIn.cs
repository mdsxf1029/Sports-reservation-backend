using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("CHECKIN")]
[SwaggerSchema(Description = "签到表")]
public class CheckIn
{
    [Key]
    [Column("APPOINTMENT_ID")]
    [SwaggerSchema("预约ID")]
    public int AppointmentId { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int? UserId { get; set; }

    [Column("CHECKIN_TIME")]
    [SwaggerSchema("签到时间")]
    public DateTime? CheckinTime { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment? Appointment { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
