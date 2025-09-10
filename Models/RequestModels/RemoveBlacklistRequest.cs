namespace Sports_reservation_backend.Models.RequestModels;

public class RemoveBlacklistRequest
{
    public int UserId { get; set; }
    public DateTime BeginTime { get; set; }
}

public class BatchRemoveBlacklistRequest
{
    public List<RemoveBlacklistRequest> BlacklistItems { get; set; } = new();
}
