using SaudiPlus.Enums;
using SaudiPlus.Response;
using SaudiPlus.Request.Policy;
using SaudiPlus.Response.Policy;
using SaudiPlus.Request.BusinessUser;

namespace SaudiPlus.Interfaces.Service
{
    public interface IPolicyService
    {
        Task<GetPolicyResponse> SetPolicy(SetPolicyDTO model);
        Task<GetPolicyResponse> GetPolicy(string type);
        Task<GetPolicyResponse> GetPolicyLocalized(string type);

        Task<GetPolicyResponse> SetBusinessPolicy(Guid businessId, SetPolicyDTO model);
        Task<GetPolicyResponse> GetBusinessPolicy(Guid businessId, string type);
        Task<GetPolicyResponse> GetBusinessPolicyLocalized(Guid businessId, string type);

        Task<SuccessResponse> SetSocialMedia(SetSocialMediaDto model);
        Task<SuccessResponse> SetContactUs(SetContactDTO model);
        Task<GetContactUsResponse> GetContactUs();
    }
}
