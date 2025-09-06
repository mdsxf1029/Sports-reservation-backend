using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.RequestModels;

[SwaggerSchema(Description = "添加申诉请求")]
public class AddAppealRequest
{
    [Required]
    [SwaggerSchema("违约记录ID")]
    public int ViolationId { get; set; }

    [Required]
    [SwaggerSchema("申诉用户ID")]
    public int UserId { get; set; }

    [Required]
    [StringLength(500)]
    [SwaggerSchema("申诉理由")]
    public string AppealReason { get; set; } = string.Empty;
}
