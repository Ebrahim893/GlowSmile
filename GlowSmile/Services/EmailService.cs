using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace GlowSmile.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // قراءة الإعدادات من appsettings.json
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"]);
            var fromEmail = _config["EmailSettings:Email"];
            var password = _config["EmailSettings:Password"];
            var senderName = _config["EmailSettings:SenderName"];

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            // هنا السر: بنحدد الـ Display Name اللي هيظهر للمستلم
            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, senderName), // "Glow Smile Clinic <email@gmail.com>"
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}