using Common.DTO.Login;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        /// Regiser new user
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="400">This email is already used</response>
        /// <response code="400">This NickName is already used</response>
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
        /// Confirming email
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User not found</response>
        /// <response code="400">Token is not valid</response>
        /// <response code="400">Confirmation date is over</response> 
        /// <response code="400">Bad request</response>
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
        /// Get token by login and password
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>  
        /// <response code="404">Iinvalid username</response>
        /// <response code="403">Email is not confirmed</response>
        /// <response code="400">Invalid password</response>
        /// <response code="400">Bad request</response>
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
        /// Get token by steamId
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>       
        /// <response code="404">User with such id not found on steam</response>       
        /// <response code="400">Bad request</response>       
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
        /// Restore password by email
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User with such email is not found</response>
        /// <response code="403">Email is not confirmed</response>
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
        /// Confirm restoring password by email
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">Token is not found</response>
        /// <response code="404">User not found or email is not confirmed</response>
        /// <response code="400">Confirmation date is over</response>
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

        //Add Phone number validation
        /// <summary>
        /// Confirm restoring password by phone number
        /// </summary>
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">User with such phone number is not found</response>
        /// <response code="403">Phone number is not confirmed</response>
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
        /// <returns></returns>
        /// <response code="500">Internal error on server</response>
        /// <response code="404">Token is not found</response>
        /// <response code="404">User not found or phone number is not confirmed</response>
        /// <response code="400">Confirmation date is over</response>
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
