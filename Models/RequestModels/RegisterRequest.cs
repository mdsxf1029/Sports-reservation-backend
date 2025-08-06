namespace Sports_reservation_backend.Models.RequestModels;

public class RegisterRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
