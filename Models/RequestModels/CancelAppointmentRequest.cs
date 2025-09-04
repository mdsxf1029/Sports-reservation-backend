namespace Sports_reservation_backend.Models.RequestModels;

public class CancelAppointmentRequest
{
    public int UserId { get; set; }
    public int AppointmentId { get; set; }
}
