using Common.Interfaces.Services;
using FacebookSubscriber;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Services.FacebookSubscriber
{
    public class FacebookSubscriberBot : IFacebookSubscriber
    {
        HardLogger _logger = new HardLogger();
        private readonly ILogger _loggerS;
        private string _path;
        public FacebookSubscriberBot(ILoggerFactory loggerFactory)
        {
            _loggerS = loggerFactory.CreateLogger<FacebookSubscriberBot>();
        }

        public void Setup(string path)
        {
            _path = Path.Combine(path, "facebookParsed.xml");          
            
            Task task1 = new Task(() => Run("developerkhoma@gmail.com", "gazdagazda", 144, 432, _logger.Logs1));
            task1.Start();

            //Task task2 = new Task(() => Run("developervovik2018@gmail.com", "gazdagazda", 100, 300, _logger.Logs2));
            //task2.Start();
        }

       

        public List<string> GetLogs1()
        {
            return _logger.Logs1;
        }

        public List<string> GetLogs2()
        {
            return _logger.Logs2;
        }
      

        private void Run(string login, string password, int min, int max, List<string> logger)
        {
            try
            {


                int MIN_TIMEOUT = min;
                int MAX_TIMEOUT = max;

                const int MAX_ERROR_COUNT = 10; //Number of errors one by one

                int Skip = 0; //will be changed while processing
                const int TAKE = 50;

                //this account is blocked by Facebook
                string LOGIN = login;
                string PASSWORD = password;

                string PATH = _path;

                Log("Starting our application", logger);
                SubscribingBot bot = null;
                try
                {
                    bot = new SubscribingBot(LOGIN, PASSWORD);
                }
                catch (Exception ex)
                {
                    Log("ERROR - " + ex.Message, logger);
                    return;
                }

                Random rnd = new Random();
                int counter = 1;
                int errorNumber = 0;
                int waitingSeconds = 0;

                Log(_path, logger);
                XElement root = null;
                try
                {
                    root = XElement.Load(_path);
                }
                catch (Exception ex)
                {
                    Log(ex.Message, logger);
                }
                var facebook = root.Elements("link").Skip(Skip).Take(TAKE);
                Log(facebook.Count().ToString(), logger);
                while (facebook.Count() != 0)
                {
                    if (facebook.Count() == 0)
                    {
                        Log("Pages to following ended", logger);
                        return;
                    }

                    foreach (var link in facebook)
                    {
                        string logMessage = "";
                        var id = link.Value.Split('/')[3];
                        logMessage += ("Processing id - " + id + ",\t counter - " + counter++ + "\n");
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        try
                        {
                            bot.Subscribe(id);
                            errorNumber = 0;
                        }
                        catch (Exception ex)
                        {
                            logMessage += (ex.Message) + "\n";
                            errorNumber++;
                        }

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        logMessage += ("Finished in " + elapsedMs + " miliseconds\n");
                        waitingSeconds = rnd.Next(MIN_TIMEOUT, MAX_TIMEOUT);
                        logMessage += ("Waiting " + waitingSeconds + " seconds\n\n");
                        Log(logMessage, logger);

                        if (errorNumber >= MAX_ERROR_COUNT)
                        {
                            Log("Max error number one by one was reached, seems like something wrong, application will be stopped", logger);
                            return;
                        }

                        Thread.Sleep(1000 * waitingSeconds);
                    }
                    Log("Processed " + facebook.Count() + " elements\nRE-AUTHORIZING", logger);
                    bot.Authorize();
                    facebook = root.Elements("link").Skip(Skip += TAKE).Take(TAKE);
                }
                Log("FINISHED", logger);
            }
            catch(Exception ex)
            {
                _loggerS.LogCritical("FUUUUCK - " + ex.Message);
            }
        }

        void Log(string message, List<string> logger)
        {
            
            logger.Add("\n" + DateTime.UtcNow + "\n" + message);
            _loggerS.LogError("\n" + DateTime.UtcNow + "\n" + message);

        }
    }
}
