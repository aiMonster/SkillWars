using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Route("api/[controller]")]
    public class CsGoLogsReceiverController : Controller
    {       
        private readonly ILogger _logger;

        public CsGoLogsReceiverController(ILoggerFactory loggerFactory)
        {            
            _logger = loggerFactory.CreateLogger<PaymentController>();
        }

        [HttpPost("Receive")]
        public async Task<IActionResult> Receive()
        {
            _logger.LogError(Request.Form.ToList().FirstOrDefault().Value);
            return Ok();
        }
    }
}
