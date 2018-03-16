using Common.Enums;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface IHtmlGeneratorService
    {
        void SetPath(string path);


        Task<string> ConfirmEmail(string link, Languages language = Languages.Eng);
        Task<string> EmailConfirmed(Languages language = Languages.Eng);

        Task<string> RestorePassword(string link, Languages language = Languages.Eng);
        Task<string> PasswordRestored(Languages language = Languages.Eng);  
    }
}
