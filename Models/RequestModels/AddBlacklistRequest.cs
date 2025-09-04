namespace Sports_reservation_backend.Models.RequestModels;

public class AddBlacklistRequest
{
    public int Id { get; set; }
    public DateTime EndTime { get; set; }
    public string BannedReason { get; set; } = string.Empty;
}
