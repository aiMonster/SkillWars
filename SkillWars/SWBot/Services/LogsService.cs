using QueryMaster.GameServer;
using QueryMaster.SWClasses;
using SWBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SWBot.Services
{
    public class LogsService : ILogService
    {
        private readonly SWLogEvents events;

        public LogsService()
        {
            events = new SWLogEvents(new IPEndPoint(IPAddress.Parse("197.192.97.3"), 27015));
            events.LogReceived += new EventHandler<LogReceivedEventArgs>(LogReceived);
        }

        private async void LogReceived(object o, LogReceivedEventArgs args)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
               { "data", args.Message}
            };
            var content = new FormUrlEncodedContent(values);
            await client.PostAsync(@"https://skillwars.azurewebsites.net/api/CsGoLogsReceiver/Receive", content);
        }

        public async Task HandleLogAsync(string logLine)
        {
            try
            {
                events.SWProcessLog(logLine);
            }
            catch(Exception ex)
            {
                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                   { "data", ex.Message}
                };
                var content = new FormUrlEncodedContent(values);
                await client.PostAsync(@"https://skillwars.azurewebsites.net/api/CsGoLogsReceiver/Receive", content);
            }
            
        }
    }
}
