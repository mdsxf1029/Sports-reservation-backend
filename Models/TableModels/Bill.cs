using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("BILL")]
[SwaggerSchema(Description = "账单表")]
public class Bill
{
    [Key]
    [Column("BILL_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SwaggerSchema(Description = "账单ID")]
    public int BillId { get; set; }
    
    [Required]
    [Column("BILL_STATUS")]
    [StringLength(20)]
    [SwaggerSchema(Description = "订单状态")]
    public string BillStatus { get; set; }=string.Empty;
    
    [Required]
    [Column("BILL_AMOUNT")]
    [SwaggerSchema(Description = "账单金额")]
    public int BillAmount { get; set; }
    
    [Required]
    [Column("BILL_TIME")]
    [SwaggerSchema(Description = "创建时间")]
    public DateTime BillTime { get; set; }
    
    [Required]
    [Column("USER_ID")]
    [SwaggerSchema(Description = "用户ID")]
    public int UserId { get; set; }
    
    [Required]
    [Column("APPOINTMENT_ID")]
    [SwaggerSchema(Description = "预约ID")]
    public int AppointmentId { get; set; }
}