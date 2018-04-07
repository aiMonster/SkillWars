using Common.DTO.Account;
using Common.DTO.Communication;
using Common.DTO.Login;
using Common.DTO.Steam;
using Common.Entity;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Services.WebSockets.Handlers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Services.LoginService
{
    public class LoginService : ILoginService
    {        
        private readonly IHtmlGeneratorService _htmlGeneratorService;
        private readonly IConfigurationRoot _configuration;
        private readonly IEmailService _emailService;
        private readonly SkillWarsContext _context;
        private readonly ISmsService _smsService;
        private readonly ILogger _logger;      

        public LoginService(SkillWarsContext context, IConfigurationRoot configurtation, IEmailService emailService, ILoggerFactory loggerFactory, IHtmlGeneratorService htmlGeneratorService, ISmsService smsService)
        {
            _logger = loggerFactory.CreateLogger<LoginService>();
            _htmlGeneratorService = htmlGeneratorService;            
            _configuration = configurtation;
            _emailService = emailService;
            _smsService = smsService;
            _context = context;          
        }

        public async Task<Response<string>> Register (RegistrationDTO request)
        {
            _logger.LogDebug($"Registering new user: {request.Email}/{request.NickName}");
            var response = new Response<string>();
            
            if(await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogDebug($"User with such email already exists: {request.Email}");
                response.Error = new Error(400, "This email is already used");
                return response;
            }
            if(await _context.Users.AnyAsync(u => u.NickName == request.NickName))
            {
                _logger.LogDebug($"User with such NickName already exists: {request.NickName}");
                response.Error = new Error(400, "This NickName is already used");
                return response;
            }

            var user = new UserEntity(request);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            response.Data = request.Email;

            var confirmationToken = Guid.NewGuid().ToString();
            await _context.Tokens.AddAsync(new TokenEntity
            {
                UserId = user.Id,
                Id = confirmationToken,                
                ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["ConfirmEmail"]))
            });
            await _context.SaveChangesAsync();

            _logger.LogDebug($"User registered, sending confirmation email: {request.Email}");
            try
            {
                var apiPath = _configuration["FrontLinks:ConfirmEmail"] + confirmationToken;
                var content = await _htmlGeneratorService.ConfirmEmail(apiPath, user.Language);
                var title = _configuration.GetSection("ConfirmEmail")[user.Language.ToString()];
                await _emailService.SendMailAsync(user.Email, content, title);
            }
            catch(Exception ex)
            {
                _logger.LogError("Couldn't send regisratigon confirmaiton email for " + user.Email + "\nException:\n" + ex.Message);
                response.Error = new Error(500, "Couldn't send, write for developers - " + ex.Message);
                return response;
            }
            _logger.LogDebug($"Registration confirmation email successfully sended: {request.Email}");
            return response;
        }       

        public async Task<Response<UserProfile>> ConfirmEmail(string confirmationToken)
        {
            _logger.LogDebug("Confirming new email");
            var response = new Response<UserProfile>();
            var token = await _context.Tokens.Where(t => t.Id == confirmationToken)
                .Include(t => t.User).FirstOrDefaultAsync();

            if (token == null)
            {
                _logger.LogDebug("Token was not found");
                response.Error = new Error(400, "Token is not valid");
                return response;
            }

            if (token.User == null)
            {
                _logger.LogDebug("User was not found already");
                response.Error = new Error(404, "User not found");
                return response;
            }

            if (token.ExpirationDate < DateTime.UtcNow)
            {
                _context.Tokens.Remove(token);
                _logger.LogDebug("Confirmation date is over");
                response.Error = new Error(400, "Confirmation date is over");
                return response;
            }

            token.User.IsEmailConfirmed = true;
            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
            response.Data = new UserProfile(token.User);

            _logger.LogDebug($"User confirmed email, sending response: {token.User.Email}");
            try
            {               
                var content = await _htmlGeneratorService.EmailConfirmed(token.User.Language);
                var title = _configuration.GetSection("ConfirmEmail")[token.User.Language.ToString()];
                await _emailService.SendMailAsync(token.User.Email, content, title);
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't notify that email confirmed for " + token.User.Email + "\nException:\n" + ex.Message);
                return response;
            }
            _logger.LogDebug($"Email confirmed successfully notified: {token.User.Email}");
           
            return response;
        }

        public async Task<Response<ClaimsIdentity>> GetIdentity(string login, string password)
        {
            _logger.LogDebug("Getting identity by login and password");
            var response = new Response<ClaimsIdentity>();

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(p => p.Email == login);
            if (user == null)
            {
                _logger.LogDebug("Invalid username");
                response.Error = new Error(404, "Invalid username");
                return response;
            }
            if(user.Password != SkillWarsEncoder.Encript(password))
            {
                _logger.LogDebug("Invalid Password");
                response.Error = new Error(400, "Invalid Password");
                return response;
            }
            if (!user.IsEmailConfirmed)
            {
                _logger.LogDebug("Email is not confirmed");
                response.Error = new Error(403, "Email is not confirmed");
                return response;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Id.ToString()),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString())
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity
                (claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            response.Data = claimsIdentity;
            return response;
        }

        public async Task<Response<ClaimsIdentity>> GetIdentity(string steamId)
        {
            _logger.LogDebug("Getting identity by steamId");
            Response<ClaimsIdentity> response = new Response<ClaimsIdentity>();

            string userName = "";
            using (var client = new HttpClient())
            {
                // Query steam user summary endpoint
                var resp = await client.GetAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_configuration["Steam:apiKey"]}&steamids={steamId}");

                if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("ApiKey for steam is broken");
                    response.Error = new Error(500, "Server is brocken, we need to update api key, write to developers");
                    return response;
                }

                var players = JsonConvert.DeserializeObject<SteamPlayerSummaryRootObject>(await resp.Content.ReadAsStringAsync()).Response.Players;
                if (players.Count == 0)
                {
                    _logger.LogError("We have got user with steam id, and couldn't find him - " + steamId);
                    response.Error = new Error(404, "User with such id is not found on the Steam");
                    return response;
                }
                userName = players[0].PersonaName;
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(p => p.SteamId == steamId);
            if (user == null)
            {
                while (await _context.Users.AllAsync(u => u.NickName == userName))
                {
                    userName += "_steam";
                }

                _context.Users.Add(new UserEntity(steamId, userName));
                await _context.SaveChangesAsync();               
            }           

            user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(p => p.SteamId == steamId);
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Id.ToString()),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString())
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity
                (claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            response.Data = claimsIdentity;
            return response;
        }

        public async Task<TokenResponse> GetToken(ClaimsIdentity identity)
        {
            _logger.LogDebug("Getting token by identity");
            var response = new Response<TokenResponse>();

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var tokenResponse = new TokenResponse
            {
                Token = encodedJwt,
                Role = identity.Claims.FirstOrDefault(r => r.Type == ClaimsIdentity.DefaultRoleClaimType).Value,
                UserId = identity.Name
            };

            return tokenResponse;
        }

        public async Task<Response<string>> RestorePasswordByEmail(string email)
        {
            _logger.LogDebug($"Restoring password by email request: " + email);
            Response<string> response = new Response<string>();

            var user = await _context.Users.FirstOrDefaultAsync(p => p.Email == email);
            if (user == null)
            {
                _logger.LogDebug("User was not found");
                response.Error = new Error(404, "User not found");
                return response;
            }
            if(!user.IsEmailConfirmed)
            {
                _logger.LogDebug("Email is not confirmed yet");
                response.Error = new Error(403, "Email is not confirmed");
                return response;
            }
            response.Data = email;

            var confirmationToken = Guid.NewGuid().ToString();
            await _context.Tokens.AddAsync(new TokenEntity
            {
                UserId = user.Id,
                Id = confirmationToken,
                ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["RestorePassword"]))
            });
            await _context.SaveChangesAsync();

            _logger.LogDebug($"User found, sending confirmation email: {user.Email}");
            try
            {
                var apiPath = _configuration["FrontLinks:RestorePassword"] + confirmationToken;
                var content = await _htmlGeneratorService.RestorePassword(apiPath, user.Language);
                var title = _configuration.GetSection("RestorePassword")[user.Language.ToString()];
                await _emailService.SendMailAsync(user.Email, content, title);
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't send restoring password by email for " + user.Email + "\nException:\n" + ex.Message);
                response.Error = new Error(500, "Couldn't send, write for developers");
                return response;
            }
            _logger.LogDebug($"Restoring password by email successfully sended: {user.Email}");

            return response;
        }

        public async Task<Response<string>> RestorePasswordByEmailConfirm(RestorePasswordRequest request)
        {
            _logger.LogDebug($"Restoring password by email" );
            Response<string> response = new Response<string>();

            var token = await _context.Tokens.Where(p => p.Id == request.Token)
                   .Include(p => p.User).FirstOrDefaultAsync();

            if (token == null)
            {
                _logger.LogDebug("Token is not valid");
                response.Error = new Error(404, "Token is not valid");
                return response;
            }

            if (token.User == null || !token.User.IsEmailConfirmed)
            {
                response.Error = new Error(404, "User not found or email is not confirmed");
                return response;
            }

            if (token.ExpirationDate < DateTime.UtcNow)
            {
                _context.Tokens.Remove(token);
                response.Error = new Error(400, "Confirmation date is over");
                return response;
            }
            response.Data = token.User.Email;

            token.User.Password = SkillWarsEncoder.Encript(request.Password);
            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();

            _logger.LogDebug($"User restored password by email: {token.User.Email}");
            try
            {
                var content = await _htmlGeneratorService.PasswordRestored(token.User.Language);
                var title = _configuration.GetSection("RestorePassword")[token.User.Language.ToString()];
                await _emailService.SendMailAsync(token.User.Email, content, title);
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't send that password restored by email for " + token.User.Email + "\nException:\n" + ex.Message);
                return response;
            }
            _logger.LogDebug($"Password restored successfully by email, sended to: {token.User.Email}");

            return response;
        }

        /*Change email to phone number*/
        public async Task<Response<string>> RestorePasswordByPhone(string phoneNumber)
        {
            _logger.LogDebug($"Restoring password by phone request: " + phoneNumber);
            Response<string> response = new Response<string>();

            var user = await _context.Users.FirstOrDefaultAsync(p => p.PhoneNumber == phoneNumber);
            if (user == null)
            {
                _logger.LogDebug("User not found");
                response.Error = new Error(404, "User not found");
                return response;
            }
            if(!user.IsPhoneNumberConfirmed)
            {
                response.Error = new Error(403, "Phone number is not confirmed");
                return response;
            }

            response.Data = phoneNumber;

            //generating identity token
            string confirmationToken;
            while(true)
            {
                Random rnd = new Random();
                confirmationToken = rnd.Next(100000, 999999).ToString();
                if(!_context.Tokens.Any(p => p.Id == confirmationToken))
                {
                    break;
                }
            }

            await _context.Tokens.AddAsync(new TokenEntity
            {
                Id = confirmationToken, 
                UserId = user.Id,
                ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["RestorePassword"]))
            });    
            await _context.SaveChangesAsync();

            _logger.LogDebug($"User found, sending sms on phone: {user.PhoneNumber}");
            try
            {
                var content = _configuration.GetSection("RestorePasswordSms")[user.Language.ToString()];
                ////==== Change email to phone number
                await _smsService.SendSms(user.Email, content.Replace("#token", confirmationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't send restoring sms for " + user.PhoneNumber + "\nException:\n" + ex.Message);
                response.Error = new Error(500, "Couldn't send, write for developers");
                return response;
            }
            _logger.LogDebug($"Restoring password by phone successfully sended: {user.PhoneNumber}");

            return response;
        }
        /*Change email to phone number*/
        public async Task<Response<string>> RestorePasswordByPhoneConfirm(RestorePasswordRequest request)
        {
            _logger.LogDebug($"Restoring password by phone");
            Response<string> response = new Response<string>();

            var token = await _context.Tokens.Where(p => p.Id == request.Token)
                  .Include(p => p.User).FirstOrDefaultAsync();

            if (token == null)
            {
                _logger.LogDebug("Token is not valid");
                response.Error = new Error(404, "Token is not valid");
                return response;
            }

            if (token.User == null || !token.User.IsPhoneNumberConfirmed)
            {
                response.Error = new Error(404, "User not found or phone number is not confirmed");
                return response;
            }

            if (token.ExpirationDate < DateTime.UtcNow)
            {
                _context.Tokens.Remove(token);
                response.Error = new Error(400, "Confirmation date is over");
                return response;
            }
            response.Data = token.User.PhoneNumber;

            token.User.Password = SkillWarsEncoder.Encript(request.Password);
            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();

            _logger.LogDebug($"User restored password: {token.User.PhoneNumber}");
            try
            {                
                var content = _configuration.GetSection("PasswordRestoredSms")[token.User.Language.ToString()];
                ////==== Change email to phone number
                await _smsService.SendSms(token.User.Email, content);
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't send that password restored for " + token.User.PhoneNumber + "\nException:\n" + ex.Message);
                return response;
            }
            _logger.LogDebug($"Password restored successfully sended: {token.User.PhoneNumber}");

            return response;
        }


        //================== FOR TESTS ONLY ========================
        public async Task<List<UserProfile>> GetAllUsers()
        {            
            return await _context.Users.Select(u => new UserProfile(u)).ToListAsync();
        }

        public async Task<Response<UserProfile>> RemoveUserById(int userId)
        {
            var response = new Response<UserProfile>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(user == null)
            {
                response.Error = new Error(404, "User not found");
                return response;
            }
            _context.Remove(user);
            await _context.SaveChangesAsync();
            response.Data = new UserProfile(user);
            return response;
        }

    }
}
