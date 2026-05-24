using System.Net;
using System.Net.Mail;

namespace E_Commerce.Common.Services
{
    public class SmtpServices
    {
        private string _email = "";
        private string _password = "";

        public void SendEmail(string subject, string email, string body)
        {
            var mail = new MailMessage();

            mail.From = new MailAddress(_email, "E-Commerce");
            mail.Subject = subject;
            mail.Body = body;
            mail.To.Add(email);
            mail.IsBodyHtml = false;

            var smtp = new SmtpClient("smtp.gmail.com")

            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(_email, _password)
            };

            smtp.Send(mail);
        }
    }
}
