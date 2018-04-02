using Common.DTO.Lobbie;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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

        /// <summary>
        /// Create lobbie
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>        
        /// <response code="403">You already participate in lobbie and can't create another</response>
        /// <response code="403">Not enough money</response>
        /// <response code="200">Success</response>
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

        /// <summary>
        /// Get all lobbies
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>        
        /// <response code="200">Success</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _lobbieService.GetAllLobbiesAsync());
        }

        /// <summary>
        /// Leave lobbie
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>  
        /// <response code="403">Seems like you don't participate here</response>
        /// <response code="403">Lobbie already started idiot</response>
        /// <response code="200">Success</response>
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

        /// <summary>
        /// Participate lobbie
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>   
        /// <response code="404">Such lobbie not found</response>
        /// <response code="403">Seems like you already participate in the any lobbie</response>
        /// <response code="403">Lobbie already started</response>
        /// <response code="403">Team is full</response>
        /// <response code="403">Not enough money</response>
        /// <response code="403">Incorrect password</response>      
        /// <response code="200">Success</response>
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
