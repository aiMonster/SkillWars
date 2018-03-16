using Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Services.SendingService
{
    public class EmailService : IEmailService
    {
        private readonly string _email;
        private readonly string _password;
        private readonly string _host;
        private static ILogger<EmailService> _logger;
        private static object _smtpLocker = new object();
        private static int _messagesSended;

        public EmailService(IConfigurationRoot root, ILogger<EmailService> logger) 
        {
            _logger = logger;
            var config = root.GetSection("SmtpConfig");
            _email = config["email"];
            _password = config["password"];
            _host = config["host"];
            _messagesSended = 0;
        }

        public async Task SendMail(string email, string message, string subject, string filePath = null)
        {
            await SendMail(new List<string> { email }, message, subject, filePath);
        }
                
        public async Task SendMail(List<string> emails, string message, string subject, string filePath = null)
        {
            using (MailMessage Message = new MailMessage()
            {
                Subject = subject,
                Body = message,
                BodyEncoding = Encoding.GetEncoding("utf-8"),
                From = new MailAddress(_email),
                IsBodyHtml = true,
            })
            {
                foreach (var email in emails)
                {
                    Message.To.Add(new MailAddress(email));
                }

                if (filePath != null)
                {
                    Message.Attachments.Add(new Attachment(filePath));
                }

                lock (_smtpLocker)
                {
                    using (SmtpClient Smtp = new SmtpClient()
                    {
                        Host = _host,
                        Credentials = new System.Net.NetworkCredential(_email, _password),
                        EnableSsl = true,
                        Timeout = 20_000                        
                    })
                    {
                        Smtp.SendCompleted += SendCompleted;
                        _logger.LogDebug($"Try send email message at: {DateTime.UtcNow}, messageId: {_messagesSended}, receiver: {_email}");
                        _messagesSended++;
                        Smtp.Send(Message);
                        Smtp.Dispose();
                    }
                }
            }
        }

        private static void SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _logger.LogDebug($"Email message send callback called at: {DateTime.UtcNow}, messageId: {_messagesSended}");
            if (e.Error != null)
            {
                _logger.LogError(0, e.Error, "Smtp Error:");
            }
        }

    }
}
