using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Response.User;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Account;
using SaudiPlus.Request.IndividualUser;
using SaudiPlus.Response.IndividualUser;

namespace SaudiPlus.Interfaces.Service
{
    public interface IIndividualUserService
    {
        Task<RegisterUserResponse> RegisterUser(RegisterUserDTO model);
        Task<UserCreateResponse> CreateIndividualUser(CreateUserDTO model);

        Task<UserEditResponse> EditUser(Guid userID, EditUserDTO model);
        Task<SuccessResponse> ToggleUserStatus(ToggleStatusDTO<Guid> model);
        Task<SuccessResponse> SetAvatar(Entities.IndividualUser user, SetUserAvatarDTO model);

        Task<GetUserResponse> GetUserById(Guid userId);

        Task<IndividualUserLoginResponse> GuestUserLogin(GuestLoginDTO model);
        Task<IndividualUserLoginResponse> LoginUser(LoginRequestDTO model);
        Task<SuccessResponse> SetDeviceToken(Entities.IndividualUser user, string  deviceToken);
        
        Task<GetOrdersResponse> GetOrders(Guid userID, EntityFilter filter);
        Task<GetPointsResponse> GetPoints(Entities.IndividualUser user);
        Task<GetAvatarResponse> GetAvatar(Entities.IndividualUser user);

        Task<SuccessResponse> SetLanguage(Entities.IndividualUser user, SetLanguageDTO model);
        Task<GetUsersResponse> GetUserByFilter(EntityFilter filter);

        Task<SuccessResponse> ToggleFavourite(Guid userId, Guid businessUserId, bool isFavourite);
        Task<GetFavouriteBusinessResponse> GetFavouriteBusiness(Guid userId, LocationDTO? location);
        Task<SuccessResponse> DeleteUser(Guid userId);
        Task<SuccessResponse> DeleteAccount(Entities.IndividualUser user);
    }
}
