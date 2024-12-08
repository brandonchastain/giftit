using System.Text.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace GiftServer.Notification
{
    public class EmailNotifier : INotifier
    {
        private ILogger<EmailNotifier> logger;
        private GiftRepository giftRepo;
        private SendGridClient emailClient;

        public EmailNotifier(ILogger<EmailNotifier> logger, GiftRepository giftRepo, string apiKey)
        {
            this.logger = logger;
            this.giftRepo = giftRepo;
            this.emailClient = new SendGridClient(apiKey);
        }

        public async Task Notify(string email, Gift gift)
        {
            bool giftStillExists = true;

            try
            {
                await this.giftRepo.GetGift(gift.Id);
            }
            catch
            {
                giftStillExists = false;
            }

            if (!giftStillExists)
            {
                this.logger.LogInformation("gift notification cancelled since it was deleted");
                return;
            }

            var body = $"Time to buy {gift.PersonName} a gift! {gift.Name} ({gift.Link})";
            var from = new EmailAddress("no-reply@brandonchastain.com");
            var subject = "Time to shop for gifts!";
            var to = new EmailAddress(email);
            var plainTextContent = body;
            var htmlContent = body;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await emailClient.SendEmailAsync(msg);
            this.logger.LogInformation(JsonSerializer.Serialize(response));
        }
    }
}