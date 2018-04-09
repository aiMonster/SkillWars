using Common.DTO.Suggestion;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SuggestionController : Controller
    {
        private readonly ISuggestionService _suggestionService;

        public SuggestionController(ISuggestionService suggestionService)
        {
            _suggestionService = suggestionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSuggestion([FromBody] SuggestionRequest request)
        {            
            return Ok(await _suggestionService.CreateSuggestionAsync(request, Convert.ToInt32(User.Identity.Name)));
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestion([FromQuery]int skip = 0, [FromQuery]int take = 15)
        {
            return Ok(await _suggestionService.GetSuggestionsAsync(skip, take, true));
        }



        [HttpPut("Confirm")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmSuggestion([FromBody]ConfirmSuggestionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _suggestionService.ConfirmSuggestionAsync(request);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

          
            return Ok(response.Data);
        }

        [HttpPut("Progress/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeProgress(int id,[FromBody] int progress)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _suggestionService.ChangeProgressAsync(id, progress);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        [HttpDelete("Decline/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeclineSuggestion(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _suggestionService.DeclineSuggestionAsync(id);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            
            return Ok(response.Data);            
        }
        
        [HttpGet("NotConfirmed")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSuggestionToConfirm([FromQuery]int skip = 0, int take = 15)
        {
            return Ok(await _suggestionService.GetSuggestionsAsync(skip, take, false));
        }
    }
}
