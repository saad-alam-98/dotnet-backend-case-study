using Microsoft.AspNetCore.Mvc;

namespace SaudiPlus.Request.BusinessUser
{
    public class MediaDTO
    {
        [FromForm(Name = "media")]
        public required List<IFormFile> MediaList { get; set; }
    }
}
