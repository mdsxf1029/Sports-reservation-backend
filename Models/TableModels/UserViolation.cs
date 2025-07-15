using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Sports_reservation_backend.Models.TableModels;

[PrimaryKey(nameof(ViolationId), nameof(UserId))]
[Table("USER_VIOLATION")]
[SwaggerSchema(Description = "用户-违约记录关联表")]
public class UserViolation
{
    [Column("VIOLATION_ID")]
    [SwaggerSchema("违约ID")]
    public int ViolationId { get; set; }

    [Column("USER_ID")]
    [SwaggerSchema("用户ID")]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(ViolationId))]
    public Violation? Violation { get; set; }
}
