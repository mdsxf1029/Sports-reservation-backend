namespace Sports_reservation_backend.Models.RequestModels
{
    public class CheckAndLockRequest
    {
        public required int VenueId { get; set; }
        public string? VenueSubname { get; set; }
        public required string Date { get; set; }
        public required int TimeSlotId { get; set; }
    }
}
