using System.Security.Claims;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Extensions
{
    public static class HttpContextExtension
    {
        public static async Task<Entities.BusinessUser?> GetBusinessFromContext(
            this HttpContext context, 
            IBusinessRepository repository
        )
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return null;
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid businessUser))
            {
                return null;
            }

            var business = await repository.GetBusinessByUserId(businessUser);
            if (business == null)
            {
                return null;
            }

            return business;
        }

        public static async Task<Entities.IndividualUser?> GetUserFromContext(
            this HttpContext context,
            IIndividualUserRepository repository
        )
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return null;
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid individualUser))
            {
                return null;
            }

            var user = await repository.GetUserByUserId(individualUser);
            if (user == null)
            {
                return null;
            }

            return user;
        }
    }
}
