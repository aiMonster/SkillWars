using Common.DTO.Lobbie;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillWars.WebSockets.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LobbieController : Controller
    {
        private readonly ILobbieService _lobbieService;
        //private readonly ChatRoomHandler _socketHandler;
        public LobbieController(ILobbieService lobbieService)//, ChatRoomHandler socketHandler)
        {
            _lobbieService = lobbieService;
            //_socketHandler = socketHandler;
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

            //send notification that new lobbie were created

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

            //await _socketHandler.SendMessageToAllAsync("fuck, it works");
            return Ok(response.Data);
        }

    }
}
