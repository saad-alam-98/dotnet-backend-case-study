using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using SaudiPlus.Interfaces;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SaudiPlus.Service
{
    public class NotificationService : INotificationService
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IAmazonSQS _sqsClient;


        private readonly string ApplicationArn;
        private readonly string SqsQueueUrl;


        public NotificationService(
            IAmazonSQS sqsClient,
            IAmazonSimpleNotificationService snsClient,
            IConfiguration configuration
        )
        {
            _sqsClient = sqsClient;
            _snsClient = snsClient;
            SqsQueueUrl = configuration["Notifications:SqsQueueUrl"]
                ?? throw new InvalidOperationException("Notifications:SqsQueueUrl must be configured.");
            ApplicationArn = configuration["Notifications:ApplicationArn"]
                ?? throw new InvalidOperationException("Notifications:ApplicationArn must be configured.");
        }

        public async Task SendNotificationAsync(
            string targetArn,
            string? title,
            string body
        )
        {
            var payload = new
            {
                @default = "New message",
                GCM = JsonSerializer.Serialize(new
                {
                    notification = new
                    {
                        title,
                        body
                    }
                })
            };

            var request = new PublishRequest
            {
                Message = JsonSerializer.Serialize(payload),
                MessageStructure = "json",
                TargetArn = targetArn
            };

            await _snsClient.PublishAsync(request);
        }

        public async Task PushNotificationAsync(string? title, string body)
        {
            var payload = JsonSerializer.Serialize(new
            {
                @default = "New message",
                GCM = JsonSerializer.Serialize(new
                {
                    notification = new
                    {
                        title,
                        body,
                        sound = "default"
                    }
                })
            });

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = SqsQueueUrl,
                MessageBody = payload
            };

            await _sqsClient.SendMessageAsync(sendMessageRequest);
        }

        public async Task<string?> CreatePlatformEndpoint(string deviceToken)
        {
            try
            {
                var response = await _snsClient.CreatePlatformEndpointAsync(new CreatePlatformEndpointRequest
                {
                    PlatformApplicationArn = ApplicationArn,
                    Token = deviceToken
                })
                .ConfigureAwait(false);

                return response.EndpointArn;
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                if (ex.ErrorCode == "InvalidParameter" && ex.Message.Contains("already exists"))
                {
                    var arn = ExtractArnFromErrorMessage(ex.Message);
                    if (!string.IsNullOrEmpty(arn))
                    {
                        return arn;
                    }
                }
                return null;
            }
        }

        private static string ExtractArnFromErrorMessage(string errorMessage)
        {
            var arnStartIndex = errorMessage.IndexOf("arn:aws:sns", StringComparison.OrdinalIgnoreCase);
            if (arnStartIndex == -1) return string.Empty;

            var arnEndIndex = errorMessage.IndexOf(' ', arnStartIndex);
            return arnEndIndex > 0 ? errorMessage.Substring(arnStartIndex, arnEndIndex - arnStartIndex) : errorMessage.Substring(arnStartIndex);
        }
    }
}
