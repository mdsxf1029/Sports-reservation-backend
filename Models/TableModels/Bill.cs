using System;
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
    [SwaggerSchema("账单ID")]
    public int BillId { get; set; }

    [Column("BILL_STATUS")]
    [StringLength(20)]
    [SwaggerSchema("支付状态，completed或pending")]
    public string? BillStatus { get; set; }

    [Column("BILL_AMOUNT")]
    [SwaggerSchema("账单金额")]
    public decimal? BillAmount { get; set; }

    [Column("BEGIN_TIME")]
    [SwaggerSchema("账单创建时间")]
    public DateTime? BeginTime { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int? UserId { get; set; }

    [Column("APPOINTMENT_ID")]
    [SwaggerSchema("预约ID")]
    public int? AppointmentId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment? Appointment { get; set; }
}
