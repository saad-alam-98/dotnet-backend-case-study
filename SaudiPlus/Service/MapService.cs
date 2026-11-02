using Newtonsoft.Json.Linq;
using SaudiPlus.Interfaces;
using System.Globalization;

namespace SaudiPlus.Service
{
    public class MapService : IMapService
    {
        private readonly string mapApiKey;

        public MapService(IConfiguration configuration)
        {
            mapApiKey = configuration["Google:MapsApiKey"]
                ?? throw new InvalidOperationException("Google:MapsApiKey must be configured.");
        }
        public async Task<(double rating, int reviewCount)> GetPlaceRating(string placeId)
        {
            var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=name,rating,user_ratings_total&key={mapApiKey}";

            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            var result = json["result"];

            return (
                result?["rating"]?.ToObject<double>() ?? 0,
                result?["user_ratings_total"]?.ToObject<int>() ?? 0
            );
        }

        public async Task<string?> GetPlaceIdFromLatLng(double lat, double lng)
        {
            var url = string.Format(CultureInfo.InvariantCulture,
                "https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&key={2}",
                lat, lng, mapApiKey
            );
            using var client = new HttpClient();

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            return json["results"]?.FirstOrDefault()?["place_id"]?.ToString();
        }
    }
}
