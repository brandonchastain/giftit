using Azure.Communication.Sms;

namespace GiftServer.Notification
{
    public class SmsNotifier : INotifier
    {
        private SmsClient smsClient;
        private const string FromPhoneNumber = "+18332475723";
        public SmsNotifier(string connectionString)
        {
            this.smsClient = new SmsClient(connectionString);
        }

        public async Task Notify(string phoneNumber, string body)
        {
            await Task.Yield();
            
            SmsSendResult sendResult = smsClient.Send(
                from: FromPhoneNumber,
                to: phoneNumber,
                message: body
            );

            Console.WriteLine($"Sms id: {sendResult.MessageId}");
        }
    }
}