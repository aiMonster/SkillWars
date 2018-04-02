using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Services.TimeredFunctionsService
{
    public class TimeredFunctionsService : ITimeredFunctionsService
    {
        private readonly ILogger _logger;
        private readonly SkillWarsContext _context;
        private readonly ILobbieService _lobbieService;

        public TimeredFunctionsService(ILoggerFactory loggerFactory, SkillWarsContext context, ILobbieService lobbieService)
        {
            _logger = loggerFactory.CreateLogger<TimeredFunctionsService>();
            _context = context;
            _lobbieService = lobbieService;
        }

        private Timer GetConfiguredTimer(int secondInterval, Func<Task> elapsedFunction)
        {
            Timer timer = new Timer()
            {
                Interval = 1000 * secondInterval,
                Enabled = true,
                AutoReset = true
            };

            timer.Elapsed += async (sender, e) => await elapsedFunction.Invoke();
            return timer;
        }

        public async Task<bool> Setup()
        {
            Timer timer = new Timer()
            {
                Interval = 1000 * 60,// 1000ms * 60 = 60sec
                Enabled = true,
                AutoReset = true
            };

            timer.Elapsed += async (sender, e) => await ((Func<Task>)Switcher).Invoke();
            timer.Start();

            return true;
        }

        private async Task Switcher()
        {
            _logger.LogInformation("Timered Functions Service started");
            var now = DateTime.UtcNow;

            if (now.Hour < 1) //once a day
            {
                await CheckEndsTokenExpirates();
            }           
            else if (now.Day == 1 && now.Hour < 1) //once a month
            {
                
            }

            //every minute
            await _lobbieService.CheckLobbies();


            _logger.LogInformation("Timered Functions Service stopped");
        }

        private async Task CheckEndsTokenExpirates()
        {
            var now = DateTime.UtcNow;
            var oldTokens = await _context.Tokens.Where(p => p.ExpirationDate < now).ToListAsync();
            _context.Tokens.RemoveRange(oldTokens);
            await _context.SaveChangesAsync();
        }
    }
}
