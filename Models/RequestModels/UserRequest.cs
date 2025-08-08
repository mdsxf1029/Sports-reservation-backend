namespace Sports_reservation_backend.Models.RequestModels;

public class UserRequest
{
    public string UserName { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string Gender { get; set; }  // male, female, unknown
    public DateTime Birthday { get; set; }
    public string AvatarUrl { get; set; }
    public string Region { get; set; }
    public string Profile { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}