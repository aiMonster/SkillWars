using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendMailAsync(string email, string message, string subject, string filePath = null);

        Task SendMailAsync(List<string> email, string message, string subject, string filePath = null);
    }
}
