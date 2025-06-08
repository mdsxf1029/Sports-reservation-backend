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
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema(Description = "预约ID")]
    public int AppointmentId { get; set; }
    
    [Required]
    [Column("APPOINTMENT_BEGIN")]
    [SwaggerSchema(Description = "预约开始时间")]
    public DateTime AppointmentBegin { get; set; }
    
    [Required]
    [Column("APPOINTMENT_STATUS")]
    [SwaggerSchema(Description = "预约状态")]
    public string AppointmentStatus { get; set; }=string.Empty;
    
}