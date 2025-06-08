using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("APPOINTMENT_CHECKIN")]
[PrimaryKey(nameof(AppointmentId),nameof(CheckInId))]
[SwaggerSchema(Description = "预约签到表")]
public class AppointmentCheckIn
{
    // 属性定义
    [Column("APPOINTMENT_ID")]
    [ForeignKey("Appointment")]
    [SwaggerSchema(Description = "预约ID")]
    public int AppointmentId { get; set; }
    
    [Column("CHECKIN_ID")]
    [ForeignKey("CheckIn")]
    [SwaggerSchema(Description = "签到ID")]
    public int CheckInId { get; set; }
    
    // 关系定义
    public Appointment? Appointment { get; set; }
    public CheckIn? CheckIn { get; set; }
}