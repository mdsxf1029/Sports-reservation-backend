using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Sports_reservation_backend.Models.RequestModels
{
    public class UploadVenueImageRequest
    {
        [Required(ErrorMessage = "必须上传图片文件")]
        public IFormFile File { get; set; }
    }
}
