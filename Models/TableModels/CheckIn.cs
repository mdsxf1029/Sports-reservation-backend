using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("CHECKIN")]
[SwaggerSchema("签到表")]
public class CheckIn
{
    [Key]
    [Column("CHECKIN_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema(Description = "签到ID")]
    public int CheckInId { get; set; }
    
    [Required]
    [Column("CHECKIN_TIME")]
    [SwaggerSchema(Description = "签到时间")]
    public DateTime CheckInTime { get; set; }
    
    [Required]
    [Column("CHECKIN_PAY_AMOUNT")]
    [SwaggerSchema(Description = "支付金额")]
    public int CheckInPayAmount { get; set; }
    
    [Required]
    [Column("CHECKIN_PAY_MODE")]
    [MaxLength(20)]
    [SwaggerSchema(Description = "支付方式")]
    public string CheckInPayMode { get; set; }=string.Empty;
    
}