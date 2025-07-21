using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("BLACKLIST")]
[SwaggerSchema(Description = "黑名单表（普通用户-管理员）")]
public class BlackList
{
    [Key]
    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [Column("MANAGER_ID")]
    [SwaggerSchema("管理员ID")]
    public int? ManagerId { get; set; }

    [Required]
    [Column("BEGIN_TIME")]
    [SwaggerSchema("开始时间")]
    public DateTime BeginTime { get; set; }

    [Required]
    [Column("END_TIME")]
    [SwaggerSchema("结束时间")]
    public DateTime EndTime { get; set; }

    [Column("BANNED_REASON")]
    [SwaggerSchema("封禁原因")]
    public string? BannedReason { get; set; }

    [Column("BANNED_STATUS")]
    [StringLength(20)]
    [SwaggerSchema("封禁状态，valid或invalid")]
    public string? BannedStatus { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(ManagerId))]
    public User? Manager { get; set; }
}
