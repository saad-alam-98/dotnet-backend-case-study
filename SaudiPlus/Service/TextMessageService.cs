using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Aws;
using Microsoft.Extensions.Options;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SaudiPlus.Service
{
    public class TextMessageService : ITextMessageService
    {
        private readonly IAmazonSimpleNotificationService _snsClient;

        public TextMessageService(IOptions<AwsCredentials> options)
        {
            _snsClient = new AmazonSimpleNotificationServiceClient(
                 options.Value.AccessKey,
                 options.Value.SecretKey,
                 new AmazonSimpleNotificationServiceConfig
                 {
                     RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(options.Value.Region)
                 }
             );
        }

        public async Task<(bool, Exception?)> SendSms(string phoneNumber, string message)
        {
            try
            {
                // Set SMS attributes for sender ID
                await SetSenderIdAttributes();
                
                var request = new PublishRequest
                {
                    Message = message,
                    PhoneNumber = phoneNumber,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        {
                            "AWS.SNS.SMS.SenderID", new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = "SaudiPlus"
                            }
                        },
                        {
                            "AWS.SNS.SMS.SMSType", new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = "Transactional"
                            }
                        }
                    }
                };

                var response = await _snsClient.PublishAsync(request);
                return (response.HttpStatusCode == System.Net.HttpStatusCode.OK, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                return (false, ex);
            }
        }
        
        private async Task SetSenderIdAttributes()
        {
            try
            {
                // Set the default sender ID at the account level
                var setAttributesRequest = new SetSMSAttributesRequest
                {
                    Attributes = new Dictionary<string, string>
                    {
                        { "DefaultSenderID", "SaudiPlus" },
                        { "DefaultSMSType", "Transactional" }
                    }
                };
                
                await _snsClient.SetSMSAttributesAsync(setAttributesRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting SMS attributes: {ex.Message}");
                // Continue despite error with attributes
            }
        }
    }
}
