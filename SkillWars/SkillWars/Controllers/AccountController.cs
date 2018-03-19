using Common.DTO.Account;
using Common.Enums;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("UserProfile")]
        public async Task<IActionResult> UserProfile()
        {
            return Ok(await _accountService.GetUserProfile(Convert.ToInt32(User.Identity.Name)));
        }

        [HttpPut("NickName")]
        public async Task<IActionResult> ChangeNickName([FromBody]string nickName)
        {
            if (String.IsNullOrEmpty(nickName))
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangeNickName(nickName, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        [HttpPut("Language")]
        public async Task<IActionResult> ChangeLanguage([FromBody]Languages language)
        {
            if (!Enum.IsDefined(typeof(Languages), language))
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangeLanguage(language, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        [HttpPost("Steam")]
        public async Task<IActionResult> ChangeSteam([FromBody]string steamId)
        {
            if (String.IsNullOrEmpty(steamId))
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangeSteamId(steamId, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        [HttpPost("Password")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangePassword(request, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        [HttpPut("Email")]
        public async Task<IActionResult> ChangeEmail([FromBody]string email)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangeEmail(email, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        [AllowAnonymous]
        [HttpGet("ConfirmEmailChanging/{token}")]
        public async Task<IActionResult> ChangeEmailConfirm([FromRoute]string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var response = await _accountService.ChangeEmailConfirm(token);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }

        //Left to add validation for phone number
        [HttpPut("PhoneNumber")]
        public async Task<IActionResult> ChangePhoneNumber([FromBody]string phoneNumber)
        {
            //if (!new EmailAddressAttribute().IsValid(email))
            //{
            //    return BadRequest(ModelState);
            //}

            var response = await _accountService.ChangeOrAddPhone(phoneNumber, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        [AllowAnonymous]
        [HttpGet("ConfirmPhoneChanging/{token}")]
        public async Task<IActionResult> ChangePhoneNumberConfirm([FromRoute]string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var response = await _accountService.ChangeOrAddPhoneConfirm(token);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }
    }
}
