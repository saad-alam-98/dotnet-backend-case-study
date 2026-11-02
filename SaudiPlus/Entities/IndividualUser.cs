using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class IndividualUser : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public required string DisplayName { get; set; } = null!;
    public string? DeviceToken { get; set; }
    public string? DeviceArn { get; set; }
    public string? PhoneNumber { get; set; } = null!;
    public string? Address { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public Language Language { get; set; }
    public string? Gender { get; set; } = null!;
    public int? CityId { get; set; }
    public int TotalPoints { get; set; }
    public string? AvatarUrl { get; set; }
    public string? OfferCode { get; set; }
    public string? LoginType { get; set; }
    public virtual City City { get; set; }
    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = [];
    public virtual ICollection<Invoice> Invoice { get; set; } = [];
    public virtual ICollection<UserOfferRedeem> RedeemedOffers { get; set; } = [];
    public virtual ICollection<Favourite> Favorites { get; set; } = [];
    public virtual ICollection<StatusInteraction> StatusInteractions { get; set; } = [];

    public string GetCityNameLocalized(Language code)
    {
        if (City == null)
            return "";

        return City.GetNameByLanguageCode(code);
    }
}
