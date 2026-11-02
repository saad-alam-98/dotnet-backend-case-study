using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Status
{
    public class EditStatusDTO
    {
        [FromForm(Name = "id")]
        public required int Id { get; set; }

        [FromForm(Name = "isActive")]
        public bool? IsActive { get; set; }

        [FromForm(Name = "title")]
        [MaxLength(50)]
        public string? Title { get; set; }

        [FromForm(Name = "media")]
        public IFormFile? Media { get; set; }

        [FromForm(Name = "duration")]
        public double? Duration { get; set; }

        [FromForm(Name = "description")]
        [MaxLength(50)]
        public string? Description { get; set; }

        [FromForm(Name = "link")]
        public string? Link { get; set; }

        [FromForm(Name = "offerId")]
        public int? OfferId { get; set; }
    }
}
