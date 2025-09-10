namespace Sports_reservation_backend.Models.RequestModels;

public class AppealCreateRequest
{
    public int UserId { get; set; }
    public int AppointmentId { get; set; }
    public string AppealReason { get; set; }
}
