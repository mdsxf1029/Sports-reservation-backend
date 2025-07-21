using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("POINT_CHANGE")]
[SwaggerSchema(Description = "积分变化表")]
public class PointChange
{
    [Key]
    [Column("CHANGE_ID")]
    [SwaggerSchema("变化ID")]
    public int ChangeId { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int? UserId { get; set; }

    [Column("CHANGE_AMOUNT")]
    [SwaggerSchema("变化数量")]
    public int? ChangeAmount { get; set; }

    [Column("CHANGE_TIME")]
    [SwaggerSchema("变化时间")]
    public DateTime? ChangeTime { get; set; }

    [Column("CHANGE_REASON")]
    [SwaggerSchema("变化原因")]
    public string? ChangeReason { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
