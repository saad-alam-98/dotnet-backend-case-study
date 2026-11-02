namespace SaudiPlus.Entities.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string? TitleAr { get; set; }
        public string? TitleEn { get; set; }
        public string MessageAr { get; set; } = string.Empty;
        public string MessageEn { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}