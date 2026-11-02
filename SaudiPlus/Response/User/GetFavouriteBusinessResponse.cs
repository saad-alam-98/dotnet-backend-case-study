using Newtonsoft.Json;

namespace SaudiPlus.Response.User
{
    public class GetFavouriteBusinessResponse : BaseResponse<GetFavouriteBusinessPayload>
    {
        public GetFavouriteBusinessResponse(
            int statusCode,
            string message,
            GetFavouriteBusinessPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetFavouriteBusinessPayload
    {
        [JsonProperty("items")]
        public required List<FavouriteBusinessItem> Items;
    }

    public class FavouriteBusinessItem
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("mobileNumber")]
        public required string MobileNumber { get; set; }

        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("logoPath")]
        public required string LogoPath { get; set; }
        
        [JsonProperty("rating")]
        public required double Rating { get; set; }

        [JsonProperty("review")]
        public required int Review { get; set; }
    }
}
