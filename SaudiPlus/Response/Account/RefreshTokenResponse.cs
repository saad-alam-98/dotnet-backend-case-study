namespace SaudiPlus.Response.Account
{
    public class RefreshTokenResponse : BaseResponse<TokenResponse>
    {
        public RefreshTokenResponse(
            int statusCode, 
            string message,
            TokenResponse? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class TokenResponse
    {
        public required string accessToken { get; set; }
        public required string refreshToken { get; set; }
        public DateTime expiry { get; set; }
    }
}
