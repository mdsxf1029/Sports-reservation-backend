namespace Sports_reservation_backend.Models.RequestModels;

public class AddViolationRequest
{
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string ViolationReason { get; set; } = null!;
    public string? ViolationPenalty { get; set; }
    public int AppointmentId { get; set; }
}
