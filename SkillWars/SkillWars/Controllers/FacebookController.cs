using Common.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Route("api/[controller]")]
    public class FacebookController : Controller
    {
        private readonly IFacebookSubscriber _facebookSubscriber;

        public FacebookController(IFacebookSubscriber facebookSubscriber)
        {
            _facebookSubscriber = facebookSubscriber;
        }

        [HttpGet("Khoma")]
        public async Task<IActionResult> GetLogs1()
        {           
            return Ok(_facebookSubscriber.GetLogs1());
        }

        [HttpGet("Vova")]
        public async Task<IActionResult> GetLogs2()
        {
            return Ok(_facebookSubscriber.GetLogs2());
        }
    }
}
