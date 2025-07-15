using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("BLACKLIST")]
[SwaggerSchema("黑名单表")]
public class BlackList
{
    [Key]
    [Column("USER_ID")]
    [ForeignKey("User")]
    [SwaggerSchema("被惩罚ID")]
    public int UserId { get; set; }
    
    [Required]
    [Column("MANAGER_ID")]
    [ForeignKey("Manager")]
    [SwaggerSchema("处理人ID")]
    public int ManagerId { get; set; }
    
    [Required]
    [Column("BEGIN_TIME")]
    [SwaggerSchema("惩罚开始时间")]
    public DateTime BeginTime { get; set; }
    
    [Required]
    [Column("END_TIME")]
    [SwaggerSchema("惩罚结束时间")]
    public DateTime EndTime { get; set; }
    
    [Required]
    [Column("BANNED_REASON")]
    [StringLength(255)]
    [SwaggerSchema("惩罚原因")]
    public string BanReason { get; set; } = string.Empty;
    
    [Required]
    [Column("BANNED_STATUS")]
    [StringLength(20)]
    [SwaggerSchema("惩罚状态")]
    public string BanStatus { get; set; } = string.Empty;
    
    public User? User { get; set; }
    public User? Manager { get; set; }
}