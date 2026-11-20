using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCapstone_TechStoreApp.Services
{
    public class EmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _email;
        private readonly string _password;

        public EmailService()
        {
            _host = Environment.GetEnvironmentVariable("SMTP_HOST");
            _email = Environment.GetEnvironmentVariable("SMTP_EMAIL");
            _password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

            string portValue =
                Environment.GetEnvironmentVariable("SMTP_PORT");

            if (string.IsNullOrWhiteSpace(_host) ||
                string.IsNullOrWhiteSpace(_email) ||
                string.IsNullOrWhiteSpace(_password) ||
                string.IsNullOrWhiteSpace(portValue))
            {
                throw new Exception("SMTP configuration is incomplete.");
            }

            _port = int.Parse(portValue);
        }

        public async Task SendVerificationCodeAsync(
            string recipient,
            string code)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(
                new MailboxAddress("Tech Store", _email));

            message.To.Add(
                new MailboxAddress("", recipient));

            message.Subject = "Tech Store Email Verification";

            message.Body = new TextPart("plain")
            {
                Text =
                    "Your Tech Store verification code is: " + code +
                    "\n\nThis code expires in 5 minutes." +
                    "\nIf you did not request this code, ignore this email."
            };

            using (SmtpClient smtp = new SmtpClient())
            {
                await smtp.ConnectAsync(
                    _host,
                    _port,
                    SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(_email, _password);

                await smtp.SendAsync(message);

                await smtp.DisconnectAsync(true);
            }
        }
    }
}
