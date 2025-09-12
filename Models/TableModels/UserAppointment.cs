using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("USER_APPOINTMENT")]
[SwaggerSchema(Description = "用户预约记录与结果通知表")]
public class UserAppointment
{
    [Key]
    [Column("APPOINTMENT_ID")]
    [SwaggerSchema("预约ID")]
    public int AppointmentId { get; set; }

    [Required]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment? Appointment { get; set; }
}
