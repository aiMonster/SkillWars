using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    public class AccountController
    {
        private readonly ILogger _logger;

        public AccountController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

    }
}
