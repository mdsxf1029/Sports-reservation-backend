using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("MAINTENANCE")]
[SwaggerSchema(Description = "维护记录表")]
public class Maintenance
{
    [Key]
    [Column("MAINTENANCE_ID")]
    [SwaggerSchema("维护ID")]
    public int MaintenanceId { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int? UserId { get; set; }

    [Column("VENUE_ID")]
    [SwaggerSchema("场地ID")]
    public int? VenueId { get; set; }

    [Column("MAINTENANCE_TIME")]
    [SwaggerSchema("维护时间")]
    public DateTime? MaintenanceTime { get; set; }

    [Column("MAINTENANCE_CONTENT")]
    [SwaggerSchema("维护内容")]
    public string? MaintenanceContent { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(VenueId))]
    public Venue? Venue { get; set; }
}
