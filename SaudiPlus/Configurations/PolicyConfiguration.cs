using SaudiPlus.Enums;
using System.Reflection;
using SaudiPlus.Constants;
using SaudiPlus.Entities.Identity;
using Microsoft.Extensions.Options;
using SaudiPlus.Helper.Application;
using Microsoft.AspNetCore.Identity;

namespace SaudiPlus.Configurations
{
    public static class PolicyConfiguration
    {
        public async static Task ConfigurePolicyService(IServiceProvider serviceProvider, IOptions<AdminCredentials> adminCredentials)
        {
            var configuration = adminCredentials.Value;

            try
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

                foreach (var roleName in RoleTranslations.RoleNames)
                {
                    var role = await EnsureRoleExists(roleManager, roleName.Value);
                }

                await EnsureAdminUserExists(userManager, roleManager, configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        private static async Task EnsureAdminUserExists(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            AdminCredentials configuration
        )
        {
            foreach (var admin in configuration.Admins)
            {
                var adminUser = new User
                {
                    UserName = admin.UserName,
                    Email = admin.UserEmail,
                    IsActive = true,
                    NormalizedEmail = admin.UserEmail.ToUpper(),
                    NormalizedUserName = admin.UserName.ToUpper(),
                    PhoneNumber = "5875495"
                };

                var existingUser = await userManager.FindByEmailAsync(admin.UserEmail);
                if (existingUser == null)
                {
                    var result = await userManager.CreateAsync(adminUser, admin.UserPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
                    }
                    else
                    {
                        throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        private static async Task<Role> EnsureRoleExists(
           RoleManager<Role> roleManager,
           (string En, string Ar) roleName
        )
        {
            var role = await roleManager.FindByNameAsync(roleName.En);
            if (role == null)
            {
                role = new Role
                {
                    Name = roleName.En,
                    NormalizedName = roleName.En.ToUpper(),
                    DescriptionAr = roleName.Ar,
                    DescriptionEn = roleName.En,
                    IsActive = true
                };
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));
                }
            }
            return role;
        }
    }
}
