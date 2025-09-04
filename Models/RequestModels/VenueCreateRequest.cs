namespace Sports_reservation_backend.Models.RequestModels;

public class VenueCreateRequest
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal Price { get; set; }
    public string Location { get; set; } = null!;
    public string OpeningHours { get; set; } = null!;
    public string BookingHours { get; set; } = null!;
    public int? MaxOccupancy { get; set; }
    public string Status { get; set; } = "开放"; // 默认开放

    public string Subname { get; set; } = null;
    public string Pictureurl { get; set; } = null;
}
