using Microsoft.AspNetCore.Identity;
using SaudiPlus.Entities;
using SaudiPlus.Entities.Identity;

namespace SaudiPlus.Extensions
{
    public static class RoleManagerExtension
    {
        public static async Task EnsureRoleExistsAsync(
            this RoleManager<Role> roleManager,
            string roleName,
            Guid creatorUserId
        )
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var newRole = new Role
                {
                    Name = roleName,
                    DescriptionAr = $"{roleName}",
                    DescriptionEn = $"{roleName}",
                    CreationTime = DateTime.UtcNow,
                    IsActive = true,
                    CreatorId = creatorUserId
                };

                var roleResult = await roleManager.CreateAsync(newRole);
                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Error creating the role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description))
                    );
                }
            }
        }
    }
}
