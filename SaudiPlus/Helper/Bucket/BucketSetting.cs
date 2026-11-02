namespace SaudiPlus.Helper.Bucket
{
    public class BucketSetting
    {
        public required string Avatar { get; set; }
        public required string Business { get; set; }
        public required string Banner { get; set; }
        public required string Category { get; set; }
        public required string Offer { get; set; }
        public required string QR { get; set; }
        public required string Partner { get; set; }
        public required string Status { get; set; }
        public string? CloudFrontDomain { get; set; }
        public string? CloudFrontDistributionId { get; set; }
    }
}
