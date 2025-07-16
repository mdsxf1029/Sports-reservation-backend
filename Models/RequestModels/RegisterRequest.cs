using System.ComponentModel.DataAnnotations;

namespace Sports_reservation_backend.Models.RequestModels
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "用户名是必填项")]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "邮箱是必填项")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [MaxLength(30)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码是必填项")]
        [MaxLength(30)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "电话号码是必填项")]
        [MaxLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [MaxLength(255)]
        public string AvatarUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "性别是必填项")]
        [RegularExpression("^(male|female|unknown)$", ErrorMessage = "性别必须为 male、female 或 unknown")]
        public string Gender { get; set; } = "unknown";

        [Required(ErrorMessage = "出生日期是必填项")]
        public DateTime Birthday { get; set; }

        [MaxLength(512)]
        public string Profile { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Region { get; set; } = string.Empty;
    }
}
