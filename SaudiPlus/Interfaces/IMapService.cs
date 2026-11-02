namespace SaudiPlus.Interfaces
{
    public interface IMapService
    {
        Task<(double rating, int reviewCount)> GetPlaceRating(string placeId);
        Task<string?> GetPlaceIdFromLatLng(double lat, double lng);
    }
}
