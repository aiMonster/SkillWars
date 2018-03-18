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
        private XmlDocument _xmlDocument;
        private readonly ILogger _logger;

        public HtmlGeneratorService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HtmlGeneratorService>();            
        }

        public void SetPath(string path)
        {
            _emailHtmlFormsPath = path;
            _xmlDocument = new XmlDocument();
            _xmlDocument.Load(_emailHtmlFormsPath + "/EmailText.xml");
        }


        public async Task<string> ConfirmEmail(string link, Languages language = Languages.Eng)
        {
            _logger.LogDebug("Generating html for ConfirmEmail");
            StringBuilder builder = new StringBuilder(File.ReadAllText(_emailHtmlFormsPath + "/Html/ConfirmEmail.html"));
            
            builder.Replace("#link", link);
            builder.Replace("#text", _xmlDocument.DocumentElement.SelectSingleNode("/Root/ButtonConfirm").Attributes[language.ToString()].Value);

            return builder.ToString();         
        }

        public async Task<string> RestorePassword(string link, Languages language = Languages.Eng)
        {
            _logger.LogDebug("Generating html for Restoring password");
            StringBuilder builder = new StringBuilder(File.ReadAllText(_emailHtmlFormsPath + "/Html/RestorePassword.html"));

            

            builder.Replace("#link", link);
            builder.Replace("#text", _xmlDocument.DocumentElement.SelectSingleNode("/Root/ButtonRestore").Attributes[language.ToString()].Value);

            return builder.ToString();
        }

        public async Task<string> EmailConfirmed(Languages language = Languages.Eng)
        {
            _logger.LogDebug("Generating html for EmailConfirmed");
            StringBuilder builder = new StringBuilder(File.ReadAllText(_emailHtmlFormsPath + "/Html/EmailConfirmed.html"));
                  
            builder.Replace("#MainText", _xmlDocument.DocumentElement.SelectSingleNode("/Root/EmailConfirmed").Attributes[language.ToString()].Value);
            return builder.ToString();
        }

        public async Task<string> PasswordRestored(Languages language = Languages.Eng)
        {
            _logger.LogDebug("Generating html for PasswordRestored");
            StringBuilder builder = new StringBuilder(File.ReadAllText(_emailHtmlFormsPath + "/Html/PasswordRestored.html"));

            builder.Replace("#MainText", _xmlDocument.DocumentElement.SelectSingleNode("/Root/PasswordRestored").Attributes[language.ToString()].Value);
            return builder.ToString();
        }

    }
}
