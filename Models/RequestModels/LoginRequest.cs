namespace Sports_reservation_backend.Models.RequestModels;

public class LoginRequest
{
    public required string Role { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public int Way { get; set; } // 预留字段，暂不处理
}
