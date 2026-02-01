using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace CornerstoneZearing.Website.Services
{
    public class SmtpService : IEmailSender
    {
        private readonly IConfiguration _Config;

        /// <summary>
        /// Initialization constructor.
        /// </summary>
        /// <param name="config"></param>
        public SmtpService(IConfiguration config)
        {
            _Config = config;
        }

        /// <summary>
        /// Sends an email message.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(string email, string subject, string html)
        {
            // From address
            string from = _Config["Smtp:From"] ?? "";
            if (string.IsNullOrEmpty(from))
            {
                throw new InvalidOperationException("SMTP from address configuration (Smtp:From) is missing.");
            }

            // Host
            string host = _Config["Smtp:Host"] ?? "";
            if (string.IsNullOrEmpty(host))
            {
                throw new InvalidOperationException("SMTP host configuration (Smtp:Host) is missing.");
            }

            // Port
            int port = 25;
            string portString = _Config["Smtp:Port"] ?? "";
            if (string.IsNullOrEmpty(portString))
            {
                if (int.TryParse(portString, out port))
                {
                    port = 25;
                }
            }

            // Credentials
            string username = _Config["Smtp:Username"] ?? "";
            string password = _Config["Smtp:Password"] ?? "";

            // SMTP client
            using (var client = new SmtpClient(host, port))
            {
                if (username.Length > 0 && password.Length > 0)
                {
                    client.Credentials = new NetworkCredential(username, password);
                }

                if (port == 465 || port == 587)
                {
                    client.EnableSsl = true;
                }

                var message = new MailMessage(from, email, subject, html)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
            }
        }
    }
}