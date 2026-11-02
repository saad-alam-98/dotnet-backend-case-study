using Newtonsoft.Json;

namespace SaudiPlus.Response.Branch
{
    public class GetBranchQRCodeResponse : BaseResponse<GetBranchQRCodePayload>
    {
        public GetBranchQRCodeResponse(
            int statusCode,
            string message,
            GetBranchQRCodePayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBranchQRCodePayload
    {
        [JsonProperty("qrCodePath")]
        public required string QrCodePath;
    }
}
