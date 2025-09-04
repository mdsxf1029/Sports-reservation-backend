namespace Sports_reservation_backend.Models.RequestModels;

public class BatchStatusRequest
{
    public List<int> Ids { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}