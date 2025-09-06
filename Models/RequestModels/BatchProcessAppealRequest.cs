using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Sports_reservation_backend.Models.RequestModels;

[SwaggerSchema(Description = "批量处理申诉请求")]
public class BatchProcessAppealRequest
{
    [Required]
    [SwaggerSchema("申诉ID列表")]
    public List<int> AppealIds { get; set; } = new();

    [Required]
    [StringLength(20)]
    [SwaggerSchema("处理动作：approve / reject")]
    public string Action { get; set; } = string.Empty;

    [StringLength(500)]
    [SwaggerSchema("拒绝理由（拒绝时必填）")]
    public string? RejectReason { get; set; }
}
