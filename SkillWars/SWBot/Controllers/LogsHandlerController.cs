using Microsoft.AspNetCore.Mvc;
using SWBot.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SWBot.Controllers
{
    [Route("api/[controller]")]
    public class LogsHandlerController : Controller
    {
        private readonly ILogService _logsService;

        public LogsHandlerController(ILogService logsService)
        {
            _logsService = logsService;
        }

        [HttpPost("Handle")]
        public async Task<IActionResult> NewTransaction([FromForm] string input)
        {
            //StreamReader reader = new StreamReader(Request.Body);
            //string text = reader.ReadToEnd();

            await _logsService.HandleLogAsync(input);            
            return Ok();
        }
    }
}
