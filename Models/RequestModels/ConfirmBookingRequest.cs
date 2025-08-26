namespace Sports_reservation_backend.Models.RequestModels
{
    public class ConfirmBookingAppointment
    {
        public required int VenueId { get; set; }
        public string? VenueSubname { get; set; }
        public required string Date { get; set; } // "YYYY-MM-DD"
        public required int TimeSlotId { get; set; }
        public required string Status { get; set; } // upcoming / ongoing ...
    }

    public class ConfirmBookingRequest
    {
        public required bool Success { get; set; }
        public required List<ConfirmBookingAppointment> Appointments { get; set; }
    }
}