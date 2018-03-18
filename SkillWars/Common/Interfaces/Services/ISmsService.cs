using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface ISmsService
    {
        Task SendSms(string phoneNumber, string message);
        Task SendSms(List<string> phoneNumbers, string message);
    }
}
