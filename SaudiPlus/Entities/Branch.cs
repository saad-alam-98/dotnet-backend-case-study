using SaudiPlus.Enums;
using SaudiPlus.Helper.Location;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class Branch : Entity<int>
{
    public required string Name { get; set; }
    public required int BranchNumber { get; set; }
    public required int CityId { get; set; }
    public required string Address { get; set; }
    public required string PlaceId { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Guid BusinessId { get; set; }
    public string? QrCodePath { get; set; }
    public virtual BusinessUser Business { get; set; }
    public virtual City City { get; set; }
    public virtual ICollection<Invoice> Invoice { get; set; } = [];

    public string GetBusinessNameLocalized(Language code)
    {
        if (Business == null)
            return "";

        return Business.GetNameByLanguageCode(code);
    }

    public string GetCityNameLocalized(Language code)
    {
        if (City == null)
            return "";

        return City.GetNameByLanguageCode(code);
    }

    public double GetBranchDistance(double? Lat, double? Long)
    {
        if (Lat != null && Long != null)
        {
            return LocationHelper.CalculateDistance(
                    (Lat ?? 0, Long ?? 0),
                    (Latitude, Longitude)
                );
        }
        else
        {
            return 0;
        }
    }
}
