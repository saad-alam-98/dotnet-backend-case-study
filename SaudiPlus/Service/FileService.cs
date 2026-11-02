using Amazon.S3;
using Amazon.S3.Model;
using Amazon.CloudFront;
using Amazon.S3.Transfer;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Aws;
using Amazon.CloudFront.Model;
using SaudiPlus.Helper.Bucket;
using Microsoft.Extensions.Options;

namespace SaudiPlus.Service
{
    public class FileService : IFileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IAmazonCloudFront _cloudFrontClient;
        private readonly AwsCredentials _credentials;
        private readonly BucketSetting _bucketSettings;

        public FileService(
            IOptions<AwsCredentials> awsOptions,
            IOptions<BucketSetting> bucketOptions)
        {
            _credentials = awsOptions.Value;
            _bucketSettings = bucketOptions.Value;
            _s3Client = new AmazonS3Client(
                _credentials.AccessKey,
                _credentials.SecretKey,
                Amazon.RegionEndpoint.GetBySystemName(_credentials.Region)
            );
            _cloudFrontClient = new AmazonCloudFrontClient(
                _credentials.AccessKey,
                _credentials.SecretKey,
                Amazon.RegionEndpoint.GetBySystemName(_credentials.Region)
            );
        }

        public async Task<(string URL, Exception? error)> UploadFileAsync(IFormFile file, string pathPrefix, int? sizeLimitInMB = null)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var uploadType = GetUploadType(pathPrefix);

            // Determine size limit in bytes
            int defaultLimitMB = uploadType == UploadType.Status ? 20 : 5;
            long maxFileSize = (sizeLimitInMB ?? defaultLimitMB) * 1024 * 1024;

            // Check file size
            if (file.Length > maxFileSize)
            {
                return ("", new Exception($"File size exceeds the {maxFileSize / (1024 * 1024)}MB limit."));
            }

            // Validate file type
            var validationError = ValidateFileType(fileExtension, uploadType);
            if (validationError != null)
                return ("", validationError);

            var bucketName = _credentials.S3BucketName;
            pathPrefix = pathPrefix.Replace(" ", "");

            // Use a unique identifier, like model.Id or model.Title, for the file name
            var key = $"{pathPrefix}/icon{fileExtension}";  // Append file extension to the file name
            var fileUrl = $"https://{bucketName}.s3.{_credentials.Region}.amazonaws.com/{key}";
            string returnUrl = fileUrl;

            if (!string.IsNullOrEmpty(_bucketSettings.CloudFrontDomain))
            {
                returnUrl = $"https://{_bucketSettings.CloudFrontDomain}/{key}";
            }

            try
            {
                // Check if the file already exists in the S3 bucket using the same key
                var fileExists = await CheckIfFileExistsAsync(bucketName, key);
                if (fileExists)
                {
                    // If the file exists, overwrite it
                    var deleteResult = await DeleteFileAsync(fileUrl);
                    if (!deleteResult.success)
                    {
                        return (fileUrl, deleteResult.error);
                    }
                }

                // Upload the file to S3
                using (var stream = file.OpenReadStream())
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        Key = key,
                        BucketName = bucketName,
                        ContentType = file.ContentType,
                    };

                    var transferUtility = new TransferUtility(_s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }

                if (!string.IsNullOrEmpty(_bucketSettings.CloudFrontDomain) &&
                    !string.IsNullOrEmpty(_bucketSettings.CloudFrontDistributionId))
                {
                    await InvalidateCloudFrontCacheAsync(key);
                }

                return (returnUrl, null);
            }
            catch (AmazonS3Exception ex)
            {
                return (returnUrl, ex);
            }
            catch (Exception ex)
            {
                return (returnUrl, ex);
            }
        }

        // Enum to represent different upload types
        private enum UploadType
        {
            Banner,
            Avatar,
            Category,
            Business,
            Partner,
            Offer,
            QR,
            General,
            Status
        }

        // Get the upload type from the path prefix
        private UploadType GetUploadType(string pathPrefix)
        {
            // Extract the first directory from the path
            var firstDir = pathPrefix.Split('/', '\\')[0];

            if (firstDir.Equals(_bucketSettings.Banner, StringComparison.OrdinalIgnoreCase))
                return UploadType.Banner;
            if (firstDir.Equals(_bucketSettings.Avatar, StringComparison.OrdinalIgnoreCase))
                return UploadType.Avatar;
            if (firstDir.Equals(_bucketSettings.Category, StringComparison.OrdinalIgnoreCase))
                return UploadType.Category;
            if (firstDir.Equals(_bucketSettings.Business, StringComparison.OrdinalIgnoreCase))
                return UploadType.Business;
            if (firstDir.Equals(_bucketSettings.Partner, StringComparison.OrdinalIgnoreCase))
                return UploadType.Partner;
            if (firstDir.Equals(_bucketSettings.Offer, StringComparison.OrdinalIgnoreCase))
                return UploadType.Offer;
            if (firstDir.Equals(_bucketSettings.QR, StringComparison.OrdinalIgnoreCase))
                return UploadType.QR;
            if (firstDir.Equals(_bucketSettings.Status, StringComparison.OrdinalIgnoreCase))
                return UploadType.Status;

            return UploadType.General;
        }

        // Validate file type based on upload type
        private Exception? ValidateFileType(string fileExtension, UploadType uploadType)
        {
            string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".gif" };
            string[] documentExtensions = { ".pdf", ".docx", ".doc", ".txt" };
            string[] videoExtensions = { ".mp4", ".mov", ".avi", ".webm", ".mkv" };

            switch (uploadType)
            {
                case UploadType.Banner:
                case UploadType.Avatar:
                case UploadType.Category:
                case UploadType.Business:
                case UploadType.Partner:
                case UploadType.Offer:
                    if (!imageExtensions.Contains(fileExtension))
                    {
                        return new Exception($"Only image files are allowed for {GetFriendlyTypeName(uploadType)}. Please upload a {string.Join(", ", imageExtensions)} file.");
                    }
                    break;

                case UploadType.Status:
                    // Status allows images and videos
                    var statusAllowed = imageExtensions.Concat(videoExtensions).ToArray();
                    if (!statusAllowed.Contains(fileExtension))
                    {
                        return new Exception($"Only image and video files are allowed for status. Supported: {string.Join(", ", statusAllowed)}");
                    }
                    break;

                case UploadType.QR:
                    if (!new[] { ".png", ".jpg", ".jpeg" }.Contains(fileExtension))
                    {
                        return new Exception("QR codes must be in PNG or JPG format.");
                    }
                    break;

                case UploadType.General:
                default:
                    var generalAllowed = imageExtensions.Concat(documentExtensions).ToArray();
                    if (!generalAllowed.Contains(fileExtension))
                    {
                        return new Exception($"Invalid file type. Only {string.Join(", ", generalAllowed)} files are allowed.");
                    }
                    break;
            }

            return null;
        }

        // Get a friendly name for the upload type
        private string GetFriendlyTypeName(UploadType uploadType)
        {
            return uploadType switch
            {
                UploadType.Banner => "banner icons",
                UploadType.Avatar => "profile avatars",
                UploadType.Category => "category icons",
                UploadType.Business => "business logos",
                UploadType.Partner => "partner logos",
                UploadType.Offer => "offer images",
                UploadType.QR => "QR codes",
                UploadType.Status => "Status",
                _ => "uploads"
            };
        }

        public async Task<(bool success, Exception? error)> DeleteFileAsync(string url)
        {
            var key = GetS3KeyFromURL(url);
            var bucketName = _credentials.S3BucketName;
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteObjectRequest);

                if (!string.IsNullOrEmpty(_bucketSettings.CloudFrontDomain) &&
                    !string.IsNullOrEmpty(_bucketSettings.CloudFrontDistributionId))
                {
                    await InvalidateCloudFrontCacheAsync(key);
                }

                return (true, null);
            }
            catch (AmazonS3Exception ex)
            {
                return (false, ex);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }

        private string GetS3KeyFromURL(string url)
        {
            var bucketName = _credentials.S3BucketName;
            var region = _credentials.Region;
            var prefix = $"https://{bucketName}.s3.{region}.amazonaws.com/";
            return url.Replace(prefix, "");
        }

        private async Task<bool> CheckIfFileExistsAsync(string bucketName, string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = key
                };
                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IFormFile CreateFormFileFromBytes(byte[] fileBytes, string fileName, string contentType)
        {
            var stream = new MemoryStream(fileBytes);
            return new FormFile(stream, 0, fileBytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private async Task InvalidateCloudFrontCacheAsync(string invalidationPath)
        {
            try
            {
                var invalidationRequest = new CreateInvalidationRequest
                {
                    DistributionId = _bucketSettings.CloudFrontDistributionId,
                    InvalidationBatch = new InvalidationBatch
                    {
                        CallerReference = DateTime.UtcNow.Ticks.ToString(),
                        Paths = new Paths
                        {
                            Quantity = 1,
                            Items = new List<string> { $"/{invalidationPath}" }
                        }
                    }
                };

                await _cloudFrontClient.CreateInvalidationAsync(invalidationRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CloudFront invalidation failed: {ex.Message}");
            }
        }
    }
}
