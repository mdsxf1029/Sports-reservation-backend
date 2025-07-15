using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Sports_reservation_backend.Models.TableModels;

[PrimaryKey(nameof(TimeSlotId), nameof(VenueId))]
[Table("VENUE_TIME_SLOT")]
[SwaggerSchema(Description = "场地与时间段关联表")]
public class VenueTimeSlot
{
    [Column("TIME_SLOT_ID")]
    [SwaggerSchema("时间段ID")]
    public int TimeSlotId { get; set; }

    [Column("VENUE_ID")]
    [SwaggerSchema("场地ID")]
    public int VenueId { get; set; }

    [Column("ACTUAL_NUMBER")]
    [SwaggerSchema("实际人数")]
    public int? ActualNumber { get; set; }

    [Column("TIME_SLOT_STATUS")]
    [MaxLength(20)]
    [SwaggerSchema("时间段状态")]
    public string? TimeSlotStatus { get; set; }

    [ForeignKey(nameof(TimeSlotId))]
    public TimeSlot? TimeSlot { get; set; }

    [ForeignKey(nameof(VenueId))]
    public Venue? Venue { get; set; }
}
