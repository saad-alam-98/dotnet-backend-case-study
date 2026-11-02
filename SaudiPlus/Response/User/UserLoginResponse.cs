using SaudiPlus.Response.City;
using SaudiPlus.Response.Account;
using Newtonsoft.Json;

namespace SaudiPlus.Response.IndividualUser
{
    public class IndividualUserLoginResponse : BaseResponse<IndividualUserLoginPayload>
    {
        public IndividualUserLoginResponse(
            int statusCode,
            string message,
            IndividualUserLoginPayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class IndividualUserLoginPayload
    {
        [JsonProperty("accessToken")]
        public required string AccessToken { get; set; }

        [JsonProperty("refreshToken")]
        public required string RefreshToken { get; set; }

        [JsonProperty("deviceToken")]
        public required string DeviceToken { get; set; }

        [JsonProperty("isPhoneConfirmed")]
        public required bool IsPhoneConfirmed { get; set; }

        [JsonProperty("expiry")]
        public required DateTime Expiry { get; set; }
    }

    public class IndividualUserData : LoginData
    {
        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }

        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonProperty("gender")]
        public required string Gender { get; set; }
        
        [JsonProperty("totalPoints")]
        public required string TotalPoints { get; set; } 
        
        [JsonProperty("avatarUrl")]
        public required string AvatarUrl { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("lastModificationTime")]
        public DateTime? LastModificationTime { get; set; }

        [JsonProperty("loginType")]
        public required string LoginType { get; set; }

        [JsonProperty("city")]
        public required CityInfoLocalized City { get; set; }

        public void SetCityNameLocalized(string name)
        {
            if (City == null)
                return;

            City.Name = name;
        }
    }
}
