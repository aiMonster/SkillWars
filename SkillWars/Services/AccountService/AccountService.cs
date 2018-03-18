using Common.DTO.Account;
using Common.DTO.Communication;
using Common.DTO.Steam;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly ILogger _logger;
        private readonly SkillWarsContext _context;
        private readonly IConfigurationRoot _configuration;

        public AccountService(SkillWarsContext context, ILoggerFactory loggerFactory, IConfigurationRoot configuration)
        {
            _logger = loggerFactory.CreateLogger<AccountService>();
            _context = context;
            _configuration = configuration;
        }

        public async Task<UserProfile> GetUserProfile(int userId)
        {           
            return new UserProfile(await _context.Users.FirstOrDefaultAsync(u => u.Id == userId));
        }

        public async Task<Response<bool>> ChangeNickName(string nickName, int userId)
        {
            _logger.LogDebug($"Changing nickName - {userId}");
            var response = new Response<bool>();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(user == null)
            {
                response.Error = new Error(500, "Unexpected error");
                return response;
            }
            if (await _context.Users.AnyAsync(u => u.NickName == nickName))
            {
                _logger.LogDebug($"User with such NickName already exists: {nickName}");
                response.Error = new Error(400, "This NickName is already used");
                return response;
            }

            user.NickName = nickName;
            await _context.SaveChangesAsync();
            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> ChangeLanguage(Languages language, int userId)
        {
            _logger.LogDebug($"Changing language - {userId}");
            var response = new Response<bool>();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                response.Error = new Error(500, "Unexpected error");
                return response;
            }

            user.Language = language;
            await _context.SaveChangesAsync();
            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> ChangeSteamId(string steamId, int userId)
        {
            _logger.LogDebug($"Changing steamId - {userId}");
            var response = new Response<bool>();

            
            if(await _context.Users.AnyAsync(u=> u.SteamId == steamId))
            {
                response.Error = new Error(403, "This steam id is already connected to any account");
                return response;
            }

            using (var client = new HttpClient())
            {
                // Query steam user summary endpoint
                var resp = await client.GetAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_configuration["Steam:apiKey"]}&steamids={steamId}");

                if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("ApiKey for steam is broken");
                    response.Error = new Error(500, "Server is brocken, we need to update api key");
                    return response;
                }

                var players = JsonConvert.DeserializeObject<SteamPlayerSummaryRootObject>(await resp.Content.ReadAsStringAsync()).Response.Players;
                if (players.Count == 0)
                {
                    _logger.LogError("We have got user with steam id, and couldn't find him while adding in profile - " + steamId);
                    response.Error = new Error(404, "User with such id is not found on the Steam");
                    return response;
                }                
            }

            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if(user == null)
            {
                response.Error = new Error(500, "Unexpected error");
                return response;
            }
            user.SteamId = steamId;
            await _context.SaveChangesAsync();
            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> ChangePassword(ChangePasswordRequest request, int userId)
        {
            _logger.LogDebug($"Changing password - {userId}");
            var response = new Response<bool>();

            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if(user == null)
            {
                response.Error = new Error(500, "Unexpected error");
                return response;
            }
            
            if(user.IsPasswordSet && user.Password != SkillWarsEncoder.Encript(request.OldPassword))
            {               
                response.Error = new Error(403, "Old password is not valid");
                return response;                           
            }
            else
            {
                user.IsPasswordSet = true;
            }
            
            user.Password = SkillWarsEncoder.Encript(request.Password);
            await _context.SaveChangesAsync();
            response.Data = true;
            return response;
        }
     }
}
