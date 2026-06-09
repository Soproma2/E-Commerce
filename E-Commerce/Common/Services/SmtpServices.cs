using System.Net;
using System.Net.Mail;

namespace E_Commerce.Common.Services
{
    public class SmtpServices
    {
        private readonly IConfiguration _config;

        public SmtpServices(IConfiguration config)
        {
            _config = config;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_config["Smtp:Email"]) &&
            !string.IsNullOrWhiteSpace(_config["Smtp:Password"]);

        public void SendEmail(string subject, string email, string body)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("SMTP service is not configured.");

            var senderEmail = _config["Smtp:Email"]!;
            var senderPassword = _config["Smtp:Password"]!;
            var host = _config["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.TryParse(_config["Smtp:Port"], out var configuredPort)
                ? configuredPort
                : 587;

            using var mail = new MailMessage();

            mail.From = new MailAddress(senderEmail, "E-Commerce");
            mail.Subject = subject;
            mail.Body = body;
            mail.To.Add(email);
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient(host)

            {
                Port = port,
                EnableSsl = true,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            smtp.Send(mail);
        }
    }
}
