using Common.DTO.Lobbie;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Authorize(Roles ="User")]
    [Route("api/[controller]")]
    public class LobbieController : Controller
    {
        private readonly ILobbieService _lobbieService;
       
        public LobbieController(ILobbieService lobbieService)
        {
            _lobbieService = lobbieService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLobbie([FromBody]LobbieRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _lobbieService.CreateLobbieAsync(request, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _lobbieService.GetAllLobbiesAsync());
        }

        [HttpPut("Leave")]
        public async Task<IActionResult> Leave()
        {
            var response = await _lobbieService.LeaveLobbieAsync(Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
           
            return Ok(response.Data);
        }

        [HttpPut("Participate")]
        public async Task<IActionResult> Participate([FromBody] ParticipatingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _lobbieService.ParticipateLobbieAsync(request, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

    }
}
