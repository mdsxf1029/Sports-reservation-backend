using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels;

[Table("USER_CHECKIN")]
[PrimaryKey(nameof(UserId),nameof(CheckInId))]
[SwaggerSchema("用户签到")]
public class UserCheckIn
{
    // 属性定义
    [Column("USER_ID")]
    [ForeignKey("User")]
    [SwaggerSchema(Description = "用户ID")]
    public int UserId { get; set; }
    
    [Column("CHECKIN_ID")]
    [ForeignKey("CheckIn")]
    [SwaggerSchema(Description = "签到ID")]
    public int CheckInId { get; set; }
    
    // 关系定义
    public User? User { get; set; }
    public CheckIn? CheckIn { get; set; }
}