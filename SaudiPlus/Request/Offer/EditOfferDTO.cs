using SaudiPlus.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Offer
{
    public class EditOfferDTO : BaseEntityDTO<int>
    {
        [FromForm(Name = "id")]
        public override required int Id { get; set; }

        [FromForm(Name = "title")]
        [MaxLength(50)]
        public string? Title { get; set; }

        [FromForm(Name = "description")]
        [MaxLength(300)]
        public string? Description { get; set; }

        [FromForm(Name = "isActive")] 
        public bool? IsActive { get; set; }

        [FromForm(Name = "offerType")]
        public OfferType? OfferType { get; set; }

        [FromForm(Name = "invoiceValue")]
        public decimal? InvoiceValue { get; set; }

        [FromForm(Name = "percentageValue")]
        public decimal? PercentageValue { get; set; }

        [FromForm(Name = "redeemByCode")]
        public bool? RedeemByCode { get; set; }

        [FromForm(Name = "redeemCode")]
        public string? RedeemCode { get; set; }

        [FromForm(Name = "notes")]
        [MaxLength(250)]
        public string? Notes { get; set; } = null!;

        [FromForm(Name = "points")]
        public int? Points { get; set; }

        [FromForm(Name = "startDate")]
        public DateTime? StartDate { get; set; }

        [FromForm(Name = "endDate")]
        public DateTime? EndDate { get; set; }

        [FromForm(Name = "logos")]
        public List<IFormFile>? Logos { get; set; }
    }
}
