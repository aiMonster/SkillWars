using Common.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SendingService
{
    public class SmsService : ISmsService
    {
        private readonly IEmailService _emailService;
        public SmsService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendSms(string phoneNumber, string message)
        {
            await SendSms(new List<string> { phoneNumber}, message);            
        }

        public async Task SendSms(List<string> phoneNumbers, string message)
        {
            foreach(var phone in phoneNumbers)
            {
                await _emailService.SendMailAsync(phone, message, "It is Sms");
            }
            
        }
    }
}
