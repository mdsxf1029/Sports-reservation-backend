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
    [Column("BEGIN_TIME")]
    [SwaggerSchema(Description = "预约开始时间")]
    public DateTime BeginTime { get; set; }
    
    [Required]
    [Column("END_TIME")]
    [SwaggerSchema(Description = "预约结束时间")]
    public DateTime EndTime { get; set; }
    
    [Required]
    [Column("APPOINTMENT_STATUS")]
    [SwaggerSchema(Description = "预约状态")]
    [StringLength(20)]
    public string AppointmentStatus { get; set; }=string.Empty;
    
    [Required]
    [Column("APPLY_TIME")]
    [SwaggerSchema(Description = "预约时间")]
    public DateTime AppointmentApplyTime { get; set; }
    
    [Required]
    [Column("FINISH_TIME")]
    [SwaggerSchema(Description = "实际结束时间")]
    public DateTime FinishTime { get; set; }
}