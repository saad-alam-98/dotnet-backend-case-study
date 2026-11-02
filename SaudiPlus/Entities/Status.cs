using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class Status : Entity<int>
    {
        public required string Title { get; set; }
        public required string MediaUrl { get; set; }
        public required bool FromAdmin { get; set; }
        public required double Duration { get; set; }
        public int Clicks { get; set; }
        public int Views { get; set; }
        public string? MediaType { get; set; }
        public string? Description { get; set; }
        public string? LinkUrl { get; set; }
        public Guid? BusinessId { get; set; }
        public int? OfferId { get; set; }
        public virtual BusinessUser? Business { get; set; }
        public virtual Offer? Offer { get; set; }
        public virtual ICollection<StatusInteraction> Interactions { get; set; } = [];


        public string GetStatusTimeAgoText()
        {
            var now = DateTime.UtcNow;
            var diff = now - CreationTime;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} minutes ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hours ago";

            var days = (int)diff.TotalDays;

            if (days == 1)
                return "Yesterday";
            if (days < 7)
                return $"{days} days ago";

            return CreationTime.ToLocalTime().ToString("MMM dd, yyyy h:mm tt");
        }

        public bool IsStatusVisible()
        {
            var expiresAt = CreationTime.AddSeconds(Duration);
            return DateTime.UtcNow <= expiresAt;
        }
    }
}
