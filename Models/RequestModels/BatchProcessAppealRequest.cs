using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sports_reservation_backend.Models.RequestModels
{
    public class BatchProcessAppealRequest
    {
        [Required(ErrorMessage = "必须提供 AppealIds")]
        [MinLength(1, ErrorMessage = "至少需要一个 AppealId")]
        public List<int> AppealIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "必须提供操作类型")]
        [RegularExpression("approve|reject", ErrorMessage = "Action 必须为 'approve' 或 'reject'")]
        public string Action { get; set; } = string.Empty;

        // 可选，只有 action=reject 时才使用
        public string? RejectReason { get; set; }
    }
}
