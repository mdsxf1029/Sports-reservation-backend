using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("VENUE")]
[SwaggerSchema(Description = "场地表")]
public class Venue
{
    [Key]
    [Column("VENUE_ID")]
    [SwaggerSchema("场地ID")]
    public int VenueId { get; set; }

    [Required]
    [Column("VENUE_NAME")]
    [StringLength(20)]
    [SwaggerSchema("场地名称")]
    public string VenueName { get; set; } = null!;

    [Required]
    [Column("VENUE_TYPE")]
    [StringLength(20)]
    [SwaggerSchema("场地类型")]
    public string VenueType { get; set; } = null!;

    [Required]
    [Column("VENUE_LOCATION")]
    [StringLength(50)]
    [SwaggerSchema("场地地点")]
    public string VenueLocation { get; set; } = null!;

    [Column("VENUE_CAPACITY")]
    [SwaggerSchema("场地容量")]
    public int? VenueCapacity { get; set; }

    [Column("VENUE_STATUS")]
    [StringLength(20)]
    [SwaggerSchema("发布状态，open或close")]
    public string? VenueStatus { get; set; }
}
