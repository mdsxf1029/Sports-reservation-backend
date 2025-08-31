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
    [Column("VENUE_SUBNAME")]
    [StringLength(20)]
    [SwaggerSchema("小场地名称")]
    public string VenueSubname { get; set; } = null!;

    [Required]
    [Column("VENUE_PICTURE_URL")]
    [StringLength(255)]
    [SwaggerSchema("场地图片")]
    public string VenuePictureUrl { get; set; } = null!;

    [Required]
    [Column("VENUE_NAME")]
    [StringLength(20)]
    [SwaggerSchema("场馆名称")]
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

    // 新增字段
    [Column("OPENING_HOURS")]
    [StringLength(50)]
    [SwaggerSchema("开放时间段")]
    public string? OpeningHours { get; set; }

    [Column("BOOKING_HOURS")]
    [StringLength(50)]
    [SwaggerSchema("提供预约服务时间段")]
    public string? BookingHours { get; set; }

    [Column("PRICE")]
    [SwaggerSchema("单价")]
    public decimal? Price { get; set; }

    [Column("PRICE_UNIT")]
    [StringLength(20)]
    [SwaggerSchema("单价对应时间单位，如小时")]
    public string? PriceUnit { get; set; }
}
