using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sports_reservation_backend.Models.RequestModels
{
    public class SubmitAppealRequest
    {
        [Required]
        public int ViolationId { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "申诉原因长度不能超过2000")]
        public string AppealReason { get; set; } = string.Empty;

        public List<string>? EvidenceUrls { get; set; }
    }
}
