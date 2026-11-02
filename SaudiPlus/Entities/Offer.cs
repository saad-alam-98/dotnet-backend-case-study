using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class OfferTagStatus
    {
        public OfferTags Type { get; set; }
        public string Tag { get; set; }
        public string TagColor { get; set; }

        public OfferTagStatus(OfferTags type, string tag, string tagColor)
        {
            Type = type;
            Tag = tag;
            TagColor = tagColor;
        }
    }

    public partial class Offer : Entity<int>
    {
        public Guid BusinessId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public OfferType OfferType { get; set; }
        public float? InvoiceValue { get; set; }
        public float? PercentageValue { get; set; }
        public bool? RedeemByCode { get; set; }
        public string? RedeemCode { get; set; }
        public required string Notes { get; set; }
        public int Points { get; set; }
        public int RedeemCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required virtual BusinessUser Business { get; set; }
        public virtual ICollection<OfferLogo> Logos { get; set; } = [];
        public virtual ICollection<UserOfferRedeem> RedeemedUsers { get; set; } = [];
        public virtual ICollection<Status> Status { get; set; } = [];

        public List<OfferTagStatus> GetOfferTagStatus()
        {
            var tags = new List<OfferTagStatus>();
            DateTime now = DateTime.UtcNow;

            bool isComingSoon = StartDate > now;
            bool isExpired = EndDate < now;
            bool isActive = !isComingSoon && !isExpired;
            bool isExpiringSoon = isActive && (EndDate - now).TotalDays <= 1;
            bool isLimitedTime = (EndDate - StartDate).TotalDays <= 14;
            bool isHot = RedeemCount > 5;

            if (isComingSoon)
            {
                tags.Add(new OfferTagStatus(OfferTags.ComingSoon, "Coming Soon", "#FFA500")); // Orange
                if (isLimitedTime)
                    tags.Add(new OfferTagStatus(OfferTags.LimitedTime, "Limited Time", "#FF8C00")); // Dark Orange
            }
            else if (isExpired)
            {
                tags.Add(new OfferTagStatus(OfferTags.Expired, "Expired", "#808080")); // Gray
            }
            else
            {
                tags.Add(new OfferTagStatus(OfferTags.Active, "Active", "#008000")); // Green
                if (isExpiringSoon)
                    tags.Add(new OfferTagStatus(OfferTags.ExpiringSoon, "Expiring Soon", "#FF4500")); // Red-Orange
                if (isLimitedTime)
                    tags.Add(new OfferTagStatus(OfferTags.LimitedTime, "Limited Time", "#FF8C00")); // Dark Orange
                if (isHot)
                    tags.Add(new OfferTagStatus(OfferTags.Hot, "Hot", "#FF0000")); // Red
            }

            return tags;
        }
    }
}
