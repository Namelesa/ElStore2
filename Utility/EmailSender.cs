using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public MailJetSettings _mailJetSettings { get; set; }
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                _mailJetSettings = _configuration.GetSection("MailJet").Get<MailJetSettings>();
                var client = new MailjetClient(_mailJetSettings.ApiKey, _mailJetSettings.SecretKey);

                var request = new MailjetRequest
                {
                    Resource = Send.Resource,
                }
                .Property(Send.FromEmail, "lllqwertylll@proton.me")
                .Property(Send.FromName, "ElStore")
                .Property(Send.Subject, subject)
                .Property(Send.HtmlPart, htmlMessage)
                .Property(Send.Recipients, new JArray {
                    new JObject {
                        { "Email", email },
                        { "Name", email }
                    }
                });

                var response = await client.PostAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {email}.", email);
                }
                else
                {
                    _logger.LogError("Failed to send email. Status: {StatusCode}, Response: {ResponseData}", response.StatusCode, response.GetData());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email to {email}.", email);
                throw; // Rethrow the exception if you want to handle it further up the call stack.
            }
        }
    }
}
