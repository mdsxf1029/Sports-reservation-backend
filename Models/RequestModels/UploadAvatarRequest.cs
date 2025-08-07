using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class UploadAvatarRequest
{
    [Required(ErrorMessage = "必须上传头像文件")]
    public IFormFile Avatar { get; set; }
}