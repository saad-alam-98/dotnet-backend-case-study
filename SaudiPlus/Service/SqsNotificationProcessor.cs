using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using SaudiPlus.Entities;
using SaudiPlus.Interfaces;
using System.Text.Json.Serialization;
using SaudiPlus.Interfaces.Repository;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SaudiPlus.Service
{
    public class SqsNotificationProcessor : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly string SqsQueueUrl;

        public SqsNotificationProcessor(
            IAmazonSQS sqsClient,
            IAmazonSimpleNotificationService snsClient,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration
        )
        {
            _sqsClient = sqsClient;
            _snsClient = snsClient;
            SqsQueueUrl = configuration["Notifications:SqsQueueUrl"]
                ?? throw new InvalidOperationException("Notifications:SqsQueueUrl must be configured.");
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = SqsQueueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 10
                    }, stoppingToken);

                    if (response?.Messages == null || response.Messages.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }

                    foreach (var message in response.Messages)
                    {
                        try
                        {
                            var payload = JsonSerializer.Deserialize<NotificationPayload>(message.Body);
                            await _sqsClient.DeleteMessageAsync(SqsQueueUrl, message.ReceiptHandle);

                            if (payload == null || string.IsNullOrWhiteSpace(payload.Title))
                            {
                                Console.WriteLine("Invalid payload. Deleting message.");
                                continue;
                            }

                            using var scope = _serviceScopeFactory.CreateScope();
                            var dbContext = scope.ServiceProvider.GetRequiredService<SaudiPlusDbContext>();
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                            var individualUserRepo = scope.ServiceProvider.GetRequiredService<IIndividualUserRepository>();

                            var users = dbContext.IndividualUser
                                .Where(u => u.IsActive && !u.IsDelete)
                                .ToList();

                            var userArns = await Task.WhenAll(users.Select(async user => new
                            {
                                DeviceArn = await EnsureDeviceArn(user, notificationService, individualUserRepo)
                            }));

                            var validArns = userArns
                                .Where(x => !string.IsNullOrEmpty(x.DeviceArn))
                                .Select(x => x.DeviceArn);

                            var notificationTasks = validArns.Select(arn =>
                                _snsClient.PublishAsync(new PublishRequest
                                {
                                    TargetArn = arn,
                                    Message = JsonSerializer.Serialize(new
                                    {
                                        @default = "New message",
                                        GCM = JsonSerializer.Serialize(new
                                        {
                                            notification = new
                                            {
                                                title = payload.Title,
                                                body = payload.Body,
                                                sound = "default"
                                            }
                                        })
                                    }),
                                    MessageStructure = "json"
                                })
                            );

                            await Task.WhenAll(notificationTasks);
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"[Message Error] {innerEx.Message}");
                            await _sqsClient.DeleteMessageAsync(SqsQueueUrl, message.ReceiptHandle);
                        }
                    }
                }
                catch (QueueDoesNotExistException)
                {
                    Console.WriteLine("[Fatal] SQS Queue does not exist.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Processor Error] {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task<string?> EnsureDeviceArn(
            Entities.IndividualUser user,
            INotificationService notificationService,
            IIndividualUserRepository userRepository
        )
        {
            if (string.IsNullOrEmpty(user.DeviceArn) && user.DeviceToken != null)
            {
                try
                {
                    var targetArn = await notificationService.CreatePlatformEndpoint(user.DeviceToken);
                    if (string.IsNullOrEmpty(targetArn))
                        return null;

                    user.DeviceArn = targetArn;
                    await userRepository.UpdateAsync(user, true);
                    return targetArn;
                }
                catch
                {
                    return null;
                }
            }
            return user.DeviceArn;
        }
    }


    public class NotificationPayload
    {
        [JsonPropertyName("default")]
        public string Default { get; set; }

        [JsonPropertyName("GCM")]
        public string GCM { get; set; }

        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(GCM)) return string.Empty;

                try
                {
                    var gcmData = JsonSerializer.Deserialize<GcmNotification>(GCM);
                    return gcmData?.Notification?.Title ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public string Body
        {
            get
            {
                if (string.IsNullOrEmpty(GCM)) return string.Empty;

                try
                {
                    var gcmData = JsonSerializer.Deserialize<GcmNotification>(GCM);
                    return gcmData?.Notification?.Body ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }

    public class GcmNotification
    {
        [JsonPropertyName("notification")]
        public GcmNotificationContent Notification { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, string> Data { get; set; }
    }

    public class GcmNotificationContent
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("sound")]
        public string Sound { get; set; } = "default";
    }
}
