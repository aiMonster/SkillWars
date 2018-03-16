using Common.Interfaces.Services;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Common.Enums;
using Microsoft.Extensions.Logging;

namespace Services.SendingService
{
    public class HtmlGeneratorService : IHtmlGeneratorService
    {
        private string _emailHtmlFormsPath;  
        private readonly ILogger _logger;

        public HtmlGeneratorService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HtmlGeneratorService>();
        }

        public void SetPath(string path)
        {
            _emailHtmlFormsPath = path;
        }


        public async Task<string> ConfirmEmail(string link, Languages language = Languages.Eng)
        {
            _logger.LogDebug("Generating html for ConfirmEmail");
            StringBuilder builder = new StringBuilder(File.ReadAllText(_emailHtmlFormsPath + "/Html/ConfirmEmail.html"));

            XmlDocument doc = new XmlDocument();
            doc.Load(_emailHtmlFormsPath + "/Text/ConfirmEmail.xml");          

            builder.Replace("#link", link);
            builder.Replace("#text", doc.DocumentElement.SelectSingleNode("/Root/ButtonText").Attributes[language.ToString()].Value);

            return builder.ToString();         
        }

        public async Task<string> RestorePassword(string link, Languages language = Languages.Eng)
        {
            _logger.LogDebug("Generating html for Restoring password");
            StringBuilder builder = new StringBuilder(File.ReadAllText(_emailHtmlFormsPath + "/Html/RestorePassword.html"));

            XmlDocument doc = new XmlDocument();
            doc.Load(_emailHtmlFormsPath + "/Text/RestorePassword.xml");

            builder.Replace("#link", link);
            builder.Replace("#text", doc.DocumentElement.SelectSingleNode("/Root/ButtonText").Attributes[language.ToString()].Value);

            return builder.ToString();
        }

        public async Task<string> EmailConfirmed(Languages language = Languages.Eng)
        {
            _logger.LogDebug("Generating html for EmailConfirmed");
            StringBuilder builder = new StringBuilder(File.ReadAllText(_emailHtmlFormsPath + "/Html/EmailConfirmed.html"));

            XmlDocument doc = new XmlDocument();
            doc.Load(_emailHtmlFormsPath + "/Text/EmailConfirmed.xml");            
            builder.Replace("#MainText", doc.DocumentElement.SelectSingleNode("/Root/MainText").Attributes[language.ToString()].Value);
            return builder.ToString();
        }

        public async Task<string> PasswordRestored(Languages language = Languages.Eng)
        {
            _logger.LogDebug("Generating html for PasswordRestored");
            StringBuilder builder = new StringBuilder(File.ReadAllText(_emailHtmlFormsPath + "/Html/PasswordRestored.html"));

            XmlDocument doc = new XmlDocument();
            doc.Load(_emailHtmlFormsPath + "/Text/PasswordRestored.xml");
            builder.Replace("#MainText", doc.DocumentElement.SelectSingleNode("/Root/MainText").Attributes[language.ToString()].Value);
            return builder.ToString();
        }

    }
}
