using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Sports_reservation_backend.Models.TableModels;

[PrimaryKey(nameof(ManagerId), nameof(VenueId))]
[Table("VENUE_MANAGER")]
[SwaggerSchema(Description = "场地-管理员职责记录表")]
public class VenueManager
{
    [Column("MANAGER_ID")]
    [SwaggerSchema("管理员ID")]
    public int ManagerId { get; set; }

    [Column("VENUE_ID")]
    [SwaggerSchema("场地ID")]
    public int VenueId { get; set; }

    [ForeignKey(nameof(ManagerId))]
    public User? Manager { get; set; }

    [ForeignKey(nameof(VenueId))]
    public Venue? Venue { get; set; }
}
