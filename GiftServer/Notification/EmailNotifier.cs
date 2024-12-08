using System.Text.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace GiftServer.Notification
{
    public class EmailNotifier : INotifier
    {
        private SendGridClient emailClient;

        public EmailNotifier(string apiKey)
        {
            this.emailClient = new SendGridClient(apiKey);
        }

        public async Task Notify(string email, string body)
        {
            var from = new EmailAddress("no-reply@brandonchastain.com");
            var subject = "Time to shop for gifts!";
            var to = new EmailAddress(email);
            var plainTextContent = body;
            var htmlContent = body;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await emailClient.SendEmailAsync(msg);
            Console.WriteLine(JsonSerializer.Serialize(response));
        }
    }
}