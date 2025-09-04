namespace Sports_reservation_backend.Models.RequestModels
{
    public class CheckInRequest
    {
        public int AppointmentId { get; set; }  // 注意大小写，C# 属性要匹配 JSON Key（用驼峰/下划线映射）
    }
}
