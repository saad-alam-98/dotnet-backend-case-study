using SaudiPlus.Enums;
using SaudiPlus.Helper.Location;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class BusinessUser : Entity<Guid>
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string NameAr { get; set; }
    public required string NameEn { get; set; }
    public required string MobileNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string LogoPath { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string? FacebookUrl { get; set; }
    public string? FacebookPage { get; set; }
    public string? InstagramUrl { get; set; }
    public string? InstagramPage { get; set; }
    public string? SnapchatUrl { get; set; }
    public string? SnapchatPage { get; set; }
    public string? TwitterUrl { get; set; }
    public string? TwitterPage { get; set; }
    public string? AndroidAppUrl { get; set; }
    public string? iOSAppUrl { get; set; }
    public string? WebsiteURL { get; set; }
    public required int CategoryId { get; set; }
    public virtual Category Category { get; set; }
    public virtual Partner Partner { get; set; }
    public virtual Contract Contract { get; set; }
    public virtual ICollection<Branch> Branches { get; set; } = [];
    public virtual ICollection<Banner> Banners { get; set; } = [];
    public virtual ICollection<Offer> Offers { get; set; } = [];
    public virtual ICollection<Invoice> Invoice { get; set; } = [];
    public virtual ICollection<BusinessPolicy> Policy { get; set; } = [];
    public virtual ICollection<Favourite> FavoritedByUsers { get; set; } = [];
    public virtual ICollection<Status> Status { get; set; } = [];

    public string GetNameByLanguageCode(Language code)
    {
        return code == Language.ar ? NameAr : NameEn;
    }

    public (Branch? Branch, double Distance) GetNearestBranchWithDistance(double? latitude, double? longitude)
    {
        if (latitude is null || longitude is null || Branches.Count == 0)
            return (null, 0);

        var nearest = Branches
            .Select(branch => new
            {
                Branch = branch,
                Distance = LocationHelper.CalculateDistance(
                    (latitude.Value, longitude.Value),
                    (branch.Latitude, branch.Longitude))
            })
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        return nearest == null ? (null, 0) : (nearest.Branch, nearest.Distance);
    }
}
