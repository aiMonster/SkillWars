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

        /// <summary>
        /// Get user profile
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>        
        /// <response code="200">Success</response>
        [HttpGet("UserProfile")]
        public async Task<IActionResult> UserProfile()
        {
            return Ok(await _accountService.GetUserProfileAsync(Convert.ToInt32(User.Identity.Name)));
        }

        /// <summary>
        /// Change NickName
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">Bad request</response>
        /// <response code="400">This NickName is already used</response>
        /// <response code="200">Success</response>
        [HttpPut("NickName")]
        public async Task<IActionResult> ChangeNickName([FromBody]string nickName)
        {
            if (String.IsNullOrEmpty(nickName))
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangeNickNameAsync(nickName, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        /// <summary>
        /// Change language
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>     
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPut("Language")]
        public async Task<IActionResult> ChangeLanguage([FromBody]Languages language)
        {
            if (!Enum.IsDefined(typeof(Languages), language))
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangeLanguageAsync(language, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        /// <summary>
        /// Change steamId
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User with such id is not found on the Steam</response>
        /// <response code="403">This steam id is already connected to any account</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPost("Steam")]
        public async Task<IActionResult> ChangeSteam([FromBody]string steamId)
        {
            if (String.IsNullOrEmpty(steamId))
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangeSteamIdAsync(steamId, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="403">Old password is not valid</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPost("Password")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangePasswordAsync(request, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        /// <summary>
        /// Change email
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">This email is already used</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPut("Email")]
        public async Task<IActionResult> ChangeEmail([FromBody]string email)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ChangeEmailAsync(email, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        /// <summary>
        /// Confirm changing email
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User not found</response>
        /// <response code="400">Token is not valid</response>
        /// <response code="400">Confirmation date is over</response>
        /// <response code="400">This email is already used</response>
        /// <response code="400">Bad request</response>
        /// <response code="202">Left to confirm old email</response>
        /// <response code="202">Left to confirm new email</response>
        /// <response code="200">Success</response>
        [AllowAnonymous]
        [HttpGet("ConfirmEmailChanging/{token}")]
        public async Task<IActionResult> ChangeEmailConfirm([FromRoute]string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var response = await _accountService.ChangeEmailConfirmAsync(token);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }


        //Left to add validation for phone number
        /// <summary>
        /// Change or add phone number
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">This phone number is already used</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPut("PhoneNumber")]
        public async Task<IActionResult> ChangePhoneNumber([FromBody]string phoneNumber)
        {
            //if (!new EmailAddressAttribute().IsValid(email))
            //{
            //    return BadRequest(ModelState);
            //}

            var response = await _accountService.ChangeOrAddPhoneAsync(phoneNumber, Convert.ToInt32(User.Identity.Name));
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        /// <summary>
        /// Confirm changing or adding phone number
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User not found</response>
        /// <response code="400">Token is not valid</response>
        /// <response code="400">Confirmation date is over</response>
        /// <response code="400">This phone number is already used</response>
        /// <response code="400">Bad request</response>
        /// <response code="202">Left to confirm old phone number</response>
        /// <response code="202">Left to confirm new phone number</response>
        /// <response code="200">Success</response>
        [AllowAnonymous]
        [HttpGet("ConfirmPhoneChanging/{token}")]
        public async Task<IActionResult> ChangePhoneNumberConfirm([FromRoute]string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var response = await _accountService.ChangeOrAddPhoneConfirmAsync(token);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }
    }
}
