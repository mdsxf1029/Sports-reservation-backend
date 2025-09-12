using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("VENUE_APPOINTMENT")]
[SwaggerSchema(Description = "场地预约记录表")]
public class VenueAppointment
{
    [Key]
    [Column("APPOINTMENT_ID")]
    [SwaggerSchema("预约ID")]
    public int AppointmentId { get; set; }

    [Required]
    [Column("VENUE_ID")]
    [SwaggerSchema("场地ID")]
    public int VenueId { get; set; }

    [ForeignKey(nameof(VenueId))]
    public Venue? Venue { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment? Appointment { get; set; }
}
