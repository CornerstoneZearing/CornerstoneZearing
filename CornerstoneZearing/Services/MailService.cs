using CornerstoneZearing.Interfaces;
using System.Net;
using System.Net.Mail;

namespace CornerstoneZearing.Services;

public class MailService(IConfiguration config) : IMailService
{
    private readonly IConfiguration _Configuration = config;

    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public async Task SendAsync(string to, string subject, string body)
    {
        var smtp = _Configuration.GetSection("Smtp");

        using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"] ?? "587"))
        {
            EnableSsl = bool.Parse(smtp["EnableSsl"] ?? "true"),
            Credentials = new NetworkCredential(smtp["Username"], smtp["Password"])
        };

        var from = new MailAddress(smtp["FromAddress"]!, smtp["FromName"]);
        using var message = new MailMessage(from, new MailAddress(to))
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        // TODO2026 add email template

        await client.SendMailAsync(message);
    }
}