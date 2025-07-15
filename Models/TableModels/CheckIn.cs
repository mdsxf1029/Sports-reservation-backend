using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("CHECKIN")]
[SwaggerSchema("签到表")]
public class CheckIn
{
    [Key]
    [Column("APPOINTMENT_ID")]
    [SwaggerSchema("订单ID")]
    public int AppointmentId { get; set; }
    
    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public DateTime UserId { get; set; }
    
    [Required]
    [Column("CHECKIN_TIME")]
    [SwaggerSchema("签到时间")]
    public DateTime CheckInTime { get; set; }
}