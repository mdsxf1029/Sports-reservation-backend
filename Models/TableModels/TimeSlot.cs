using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("TIME_SLOT")]
[SwaggerSchema(Description = "开放时间段表")]
public class TimeSlot
{
    [Key]
    [Column("TIME_SLOT_ID")]
    [SwaggerSchema("开放时间段ID")]
    public int TimeSlotId { get; set; }

    [Column("BEGIN_TIME")]
    [SwaggerSchema("时段起始时间")]
    public DateTime? BeginTime { get; set; }

    [Column("END_TIME")]
    [SwaggerSchema("时段结束时间")]
    public DateTime? EndTime { get; set; }
}
