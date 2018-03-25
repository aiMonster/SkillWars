using Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{    
    [Route("api/[controller]")]
    public class ForTestsOnlyController : Controller
    {
        private readonly ILoginService _loginService;

        public ForTestsOnlyController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [AllowAnonymous]
        [HttpGet("Users")]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _loginService.GetAllUsers());
        }

        [AllowAnonymous]
        [HttpDelete("Users/{id}")]
        public async Task<IActionResult> RemoveUserById(int id)
        {
            var response = await _loginService.RemoveUserById(id);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }

        [HttpGet("ForAuthorized")]
        [Authorize]
        public async Task<string> OnlyAuthorized()
        {
            return "You are authorized user or admin";
        }

        [HttpGet("ForUser")]
        [Authorize(Roles ="User")]
        public async Task<string> OnlyForUser()
        {
            return "Only for user";
        }

        [HttpGet("ForAdmin")]
        [Authorize(Roles ="Admin")]
        public async Task<string> OnlyForAdmin()
        {
            return "Only for admin";
        }

        [AllowAnonymous]
        [HttpGet("HardLogs")]
        public async Task<List<string>> GetLogs()
        {
            return HardLogger.logs;
        }



    }
}
