using System.ComponentModel.DataAnnotations;

namespace Sports_reservation_backend.Models.RequestModels
{
    public class ProcessAppealRequest
    {
        [Required]
        [RegularExpression("approve|reject", ErrorMessage = "Action 必须为 approve 或 reject")]
        public string Action { get; set; } = string.Empty;

        // 当 Action 为 reject 时，可以填写拒绝理由，可选
        public string? RejectReason { get; set; }
    }
}
