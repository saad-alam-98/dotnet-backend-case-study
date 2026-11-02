using SaudiPlus.Entities;
using SaudiPlus.Helper.Identity;
using Microsoft.AspNetCore.Identity;
using SaudiPlus.Entities.Identity;

namespace SaudiPlus.Configurations
{
    public static class IdentityConfiguration
    {
        public static void ConfigureIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<User, Role>(option =>
            {
                option.User.RequireUniqueEmail = true;
                option.Password.RequireNonAlphanumeric = false;
            })
            .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
            .AddEntityFrameworkStores<SaudiPlusDbContext>()
            .AddDefaultTokenProviders();
        }
    }
}
