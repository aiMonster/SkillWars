using Common.DTO.Account;
using Common.DTO.Login;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;       

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;            
        }

        /// <summary>
        /// Registration
        /// </summary>
        /// <remarks>
        /// Here we can register new user
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]RegistrationDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _loginService.Register(request);
            if(response.Error != null)
            {                
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        /// <summary>
        /// Registration by steam
        /// </summary>
        /// <remarks>
        /// Here we can register new user by steam
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">Bad request</response>
        /// <response code="404">User wiht such id is not found</response>
        /// <response code="200">Success</response>
        [HttpPost("SteamRegister")]
        public async Task<IActionResult> SteamRegister([FromBody]SteamRegistrationDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _loginService.SteamRegister(request);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }

        /// <summary>
        /// Confirming email
        /// </summary>
        /// <remarks>
        /// Here we can confirm email by token that was sent on email
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Email is already confirmed</response>
        /// <response code="200">Success</response>
        [HttpGet("ConfirmEmail/{token}")]
        public async Task<IActionResult> ConfirmEmail([FromRoute]string token)
        {
            if(String.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var response = await _loginService.ConfirmEmail(token);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }

        /// <summary>
        /// Getting token
        /// </summary>
        /// <remarks>
        /// Here we can get token to get access for authorithed methods
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">Bad request</response>
        /// <response code="404">Iinvalid username or password</response>
        /// <response code="403">Email is not confirmed</response>
        /// <response code="200">Success</response>
        [HttpPost("Token")]
        public async Task<IActionResult> Token([FromBody]LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identity = await _loginService.GetIdentity(request.Login, request.Password);
            if (identity.Error != null)
            {
                return StatusCode(identity.Error.ErrorCode, identity.Error);
            }

            var response = await _loginService.GetToken(identity.Data);
            return Ok(response);
        }

        /// <summary>
        /// Getting token by steam id
        /// </summary>
        /// <remarks>
        /// Here we can get token to get access for authorithed methods by steam id
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">Bad request</response>
        /// <response code="404">User with such id not found on steam</response>
        /// <response code="403">Email is not confirmed or not set</response>
        /// <response code="200">Left to register email</response>
        /// <response code="200">Success</response>
        [HttpPost("Token/{steamId}")]
        public async Task<IActionResult> TokenBySteam([FromRoute]string steamId)
        {
            if(String.IsNullOrEmpty(steamId))
            {
                return BadRequest();
            }

            var identity = await _loginService.GetIdentity(steamId);
            if (identity.Error != null)
            {
                return StatusCode(identity.Error.ErrorCode, identity.Error);
            }

            return Ok(await _loginService.GetToken(identity.Data));
        }

        /// <summary>
        /// Restoring password by email
        /// </summary>
        /// <remarks>
        /// Here we can restore password by email if user forgot it
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User with such email is not found</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPut("RestorePasswordByEmail")]
        public async Task<IActionResult> RestorePasswordByEmail([FromBody] string email)
        {
            if(String.IsNullOrEmpty(email))
            {
                return BadRequest();
            }

            var response = await _loginService.RestorePasswordByEmail(email);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }

        /// <summary>
        /// Restoring password by email confirmation
        /// </summary>
        /// <remarks>
        /// Here we can restore password by got on email token
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User or token is not found</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPut("RestorePasswordByEmailConfirm")]
        public async Task<IActionResult> RestorePasswordByEmailConfirm([FromBody] RestorePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _loginService.RestorePasswordByEmailConfirm(request);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }

        /// <summary>
        /// Restoring password by phone number
        /// </summary>
        /// <remarks>
        /// Here we can restore password by sms if user forgot it
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User with such phone number is not found</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPut("RestorePasswordByPhone")]
        public async Task<IActionResult> RestorePasswordByPhone([FromBody] string phoneNumber)
        {
            if (String.IsNullOrEmpty(phoneNumber))
            {
                return BadRequest();
            }

            var response = await _loginService.RestorePasswordByPhone(phoneNumber);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }

        /// <summary>
        /// Restoring password by phone confirmation
        /// </summary>
        /// <remarks>
        /// Here we can restore password by got on phone code
        /// </remarks>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User or token is not found</response>
        /// <response code="400">Bad request</response>
        /// <response code="200">Success</response>
        [HttpPut("RestorePasswordByPhoneConfirm")]
        public async Task<IActionResult> RestorePasswordByPhoneConfirm([FromBody] RestorePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _loginService.RestorePasswordByPhoneConfirm(request);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }
            return Ok(response.Data);
        }

    }
}
