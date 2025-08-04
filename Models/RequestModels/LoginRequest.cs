using System.ComponentModel.DataAnnotations;

namespace Sports_reservation_backend.Models.RequestModels;

public class LoginRequest
{
    [Required(ErrorMessage = "邮箱是必填项")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [MaxLength(30)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码是必填项")]
    [MaxLength(30)]
    public string Password { get; set; } = string.Empty;
}
