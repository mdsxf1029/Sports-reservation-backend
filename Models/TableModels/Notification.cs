using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.TableModels
{
    [Table("NOTIFICATION")]
    [SwaggerSchema(Description = "通知表")]
    public class Notification
    {
        [Key]
        [Column("NOTIFICATION_ID")]
        [SwaggerSchema("通知ID")]
        public int NotificationId { get; set; }

        [Column("USER_ID")]
        [SwaggerSchema("用户ID")]
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Column("CONTENT")]
        [StringLength(255)]
        [SwaggerSchema("通知内容")]
        public string? Content { get; set; }

        [Column("ISREAD")]
        [SwaggerSchema("阅读状态，0未读，1已读")]
        public int? IsRead { get; set; }

        [Column("CREATETIME")]
        [SwaggerSchema("创建时间")]
        public DateTime? CreateTime { get; set; }
    }
}
