namespace SaudiPlus.Helper.Aws
{
    public class AwsCredentials
    {
        public required string Profile { get; set; }
        public required string S3BucketName { get; set; }
        public required string AccessKey { get; set; }
        public required string SecretKey { get; set; }
        public required string Region { get; set; }
    }
}
