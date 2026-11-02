using SaudiPlus.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Offer
{
    public interface IOfferDTO
    {
        string Title { get; set; }
        string Description { get; set; }
        OfferType OfferType { get; set; }
        decimal? InvoiceValue { get; set; }
        decimal? PercentageValue { get; set; }
        bool RedeemByCode { get; set; }
        string? RedeemCode { get; set; }
        string Notes { get; set; }
        int Points { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        List<IFormFile> Logos { get; set; }
    }

    public class CreateOfferDTO : IOfferDTO
    {
        [FromForm(Name = "title")]
        [MaxLength(50)]
        public required string Title { get; set; }

        [FromForm(Name = "description")]
        [MaxLength(300)]
        public required string Description { get; set; }

        [FromForm(Name = "offerType")]
        public required OfferType OfferType { get; set; }

        [FromForm(Name = "invoiceValue")]
        public decimal? InvoiceValue { get; set; }

        [FromForm(Name = "percentageValue")]
        public decimal? PercentageValue { get; set; }

        [FromForm(Name = "redeemByCode")]
        public bool RedeemByCode { get; set; }

        [FromForm(Name = "redeemCode")]
        public string? RedeemCode { get; set; }

        [FromForm(Name = "notes")]
        [MaxLength(250)]
        public string Notes { get; set; } = null!;

        [FromForm(Name = "points")]
        public required int Points { get; set; }

        [FromForm(Name = "startDate")]
        public DateTime StartDate { get; set; }

        [FromForm(Name = "endDate")]
        public DateTime EndDate { get; set; }

        [FromForm(Name = "logos")]
        public required List<IFormFile> Logos { get; set; }
    }

    public class AdminCreateOfferDTO : CreateOfferDTO
    {
        [FromForm(Name = "businessId")]
        public required Guid BusinessId { get; set; }
    }
}
