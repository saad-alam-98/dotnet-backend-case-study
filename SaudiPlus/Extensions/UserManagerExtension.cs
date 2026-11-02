using System.Data;
using SaudiPlus.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace SaudiPlus.Extensions
{
    public static class UserManagerExtension
    {
        public static async Task<Exception?> ValidateEmailUniqueness(
            this UserManager<User> userManager,
            string email
        ) 
        {
            var existingUserByEmail = await userManager.FindByEmailAsync(email);
            if (existingUserByEmail != null)
            {
                var error = userManager.ErrorDescriber.DuplicateEmail(email);
                return new Exception(error.Description);
            }
            return null;
        }

        public static async Task<Exception?> ValidateUsernameUniqueness(
            this UserManager<User> userManager,
            string username
        )
        {
            var existingUserByUsername = await userManager.FindByNameAsync(username);
            if (existingUserByUsername != null)
            {
                var error = userManager.ErrorDescriber.DuplicateUserName(username);
                return new Exception(error.Description);
            }
            return null;
        }

        public static async Task AddUserToRole(
            this UserManager<User> userManager,
            User user, 
            string roleName
        )
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addToRoleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                throw new Exception(
                    "Error assigning the role to the user: " + 
                    string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))
                );
            }
        }


        public static async Task<string> GetUserRolesByEmailAsync(
            this UserManager<User> userManager,
            string email
        )
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return string.Empty; // User not found
            }

            var roles = await userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }
    }
}
