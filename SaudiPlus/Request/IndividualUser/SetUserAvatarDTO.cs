using Microsoft.AspNetCore.Mvc;

namespace SaudiPlus.Request.IndividualUser
{
    public class SetUserAvatarDTO
    {
        [FromForm(Name = "avatar")]
        public required IFormFile Avatar { get; set; }
    }
}
