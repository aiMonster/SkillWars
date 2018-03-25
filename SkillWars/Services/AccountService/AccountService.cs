using Common.DTO.Account;
using Common.DTO.Communication;
using Common.DTO.Steam;
using Common.Entity;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Services.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly ILogger _logger;
        private readonly SkillWarsContext _context;
        private readonly IConfigurationRoot _configuration;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;      
        private readonly IHtmlGeneratorService _htmlGeneratorService;

        public AccountService(SkillWarsContext context, ILoggerFactory loggerFactory, IConfigurationRoot configuration, IHtmlGeneratorService htmlGeneratorService, ISmsService smsService, IEmailService emailService)
        {
            _logger = loggerFactory.CreateLogger<AccountService>();
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _smsService = smsService;          
            _htmlGeneratorService = htmlGeneratorService;
        }

        public async Task<UserProfile> GetUserProfile(int userId) => new UserProfile(await _context.Users.FirstOrDefaultAsync(u => u.Id == userId));       

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
               
                if (JsonConvert.DeserializeObject<SteamPlayerSummaryRootObject>(await resp.Content.ReadAsStringAsync()).Response.Players.Count == 0)
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

        public async Task<Response<bool>> ChangeEmail(string email, int userId)
        {
            _logger.LogDebug($"Changing email - {userId}");
            var response = new Response<bool>();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                response.Error = new Error(500, "Unexpected error");
                return response;
            }
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                _logger.LogDebug($"User with such email already exists: {email}");
                response.Error = new Error(400, "This Email is already used");
                return response;
            }

            //var confirmationToken = Guid.NewGuid().ToString();
            //await _context.Tokens.AddAsync(new TokenEntity
            //{
            //    //UserId = user.Id,
            //    //Id = confirmationToken,
            //    //ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["ConfirmEmail"])),
            //    AdditionalInfo = email
            //});
            //await _context.SaveChangesAsync();

            var oldTokenId = Guid.NewGuid().ToString();
            var newTokenId = Guid.NewGuid().ToString();
            TokenEntity oldEmailToken = new TokenEntity
            {
                UserId = user.Id,
                Id = oldTokenId,
                ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["ConfirmEmail"])),
                AdditionalInfo = JsonConvert.SerializeObject(new ChangingContactsDTO { ContactType = ConactTypes.Old, AnotherTokenId = newTokenId, NewContact = email })
            };
            TokenEntity newEmailToken = new TokenEntity
            {
                UserId = user.Id,
                Id = newTokenId,
                ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["ConfirmEmail"])),
                AdditionalInfo = JsonConvert.SerializeObject(new ChangingContactsDTO { ContactType = ConactTypes.New, AnotherTokenId = oldTokenId, NewContact = email })
            };
            await _context.AddAsync(oldEmailToken);
            await _context.AddAsync(newEmailToken);
            await _context.SaveChangesAsync();

            _logger.LogDebug($"User requested to change email, left send confirmation emails: {email}");
            try
            {
                var newApiPath = _configuration["FrontLinks:ChangeEmail"] + newEmailToken.Id;
                var newContent = await _htmlGeneratorService.ConfirmEmail(newApiPath, user.Language);

                var oldApiPath = _configuration["FrontLinks:ChangeEmail"] + oldEmailToken.Id;
                var oldContent = await _htmlGeneratorService.ConfirmEmail(oldApiPath, user.Language);

                var title = _configuration.GetSection("ConfirmEmail")[user.Language.ToString()];
                await _emailService.SendMail(email, newContent, title);
                await _emailService.SendMail(user.Email, oldContent, title);
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't send confirmaiton email for " + email + " or " + user.Email +  "\nException:\n" + ex.Message);
                response.Error = new Error(500, "Couldn't send, write for developers - " + ex.Message);
                return response;
            }
            _logger.LogDebug($"Confirmation email successfully sended: {email}");
            
            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> ChangeEmailConfirm(string confirmationToken)
        {
            _logger.LogDebug("Confirming new email");
            var response = new Response<bool>();
            var token = await _context.Tokens.Where(t => t.Id == confirmationToken)
                .Include(t => t.User).FirstOrDefaultAsync();

            if (token == null)
            {
                response.Error = new Error(400, "Token is not valid");
                return response;
            }

            if (token.User == null)
            {
                response.Error = new Error(404, "User not found");
                return response;
            }

            if (token.ExpirationDate < DateTime.UtcNow)
            {
                _context.Tokens.Remove(token);
                response.Error = new Error(400, "Confirmation date is over");
                return response;
            }

            var additionalInfo = JsonConvert.DeserializeObject<ChangingContactsDTO>(token.AdditionalInfo);
            var anotherToken = await _context.Tokens.Where(t => t.Id == additionalInfo.AnotherTokenId).FirstOrDefaultAsync();
            var anotherAdditionalInfo = JsonConvert.DeserializeObject<ChangingContactsDTO>(anotherToken.AdditionalInfo);

            if (await _context.Users.AnyAsync(u => u.Email == additionalInfo.NewContact))
            {
                _logger.LogDebug($"User with such email already exists: {additionalInfo.NewContact}");
                response.Error = new Error(400, "This Email is already used");
                return response;
            }

            switch (additionalInfo.ContactType)
            {
                case ConactTypes.New:
                    additionalInfo.IsNewContactConfirmed = true;
                    anotherAdditionalInfo.IsNewContactConfirmed = true;
                    break;
                case ConactTypes.Old:
                    additionalInfo.IsOldContactConfirmed = true;
                    anotherAdditionalInfo.IsOldContactConfirmed = true;
                    break;
            }

            token.AdditionalInfo = JsonConvert.SerializeObject(additionalInfo);
            anotherToken.AdditionalInfo = JsonConvert.SerializeObject(anotherAdditionalInfo);
            await _context.SaveChangesAsync();

            if(!additionalInfo.IsOldContactConfirmed)
            {
                response.Error = new Error(202, "Left to confirm old email");
                return response;
            }
            else if(!additionalInfo.IsNewContactConfirmed)
            {
                response.Error = new Error(202, "Left to confirm new email");
                return response;
            }

            token.User.Email = additionalInfo.NewContact;
            _context.Tokens.Remove(token);
            _context.Tokens.Remove(anotherToken);
            await _context.SaveChangesAsync();
            response.Data = true;

            _logger.LogDebug($"User confirmed email: {token.User.Email}");
            try
            {
                var content = await _htmlGeneratorService.EmailConfirmed(token.User.Language);
                var title = _configuration.GetSection("ConfirmEmail")[token.User.Language.ToString()];
                await _emailService.SendMail(token.User.Email, content, title);
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't send that email confirmed for " + token.User.Email + "\nException:\n" + ex.Message);
                return response;
            }
            _logger.LogDebug($"Email confirmed successfully sended: {token.User.Email}");

            return response;
        }

        /*Change email to phone number*/
        public async Task<Response<bool>> ChangeOrAddPhone(string phoneNumber, int userId)
        {
            _logger.LogDebug($"Changing or adding phone - {userId}");
            var response = new Response<bool>();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                response.Error = new Error(500, "Unexpected error");
                return response;
            }
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
            {
                _logger.LogDebug($"User with such phoneNumber already exists: {phoneNumber}");
                response.Error = new Error(400, "This Phone Number is already used");
                return response;
            }

            //generating identity token
            string oldTokenId;
            while (true)
            {
                Random rnd = new Random();
                oldTokenId = rnd.Next(100000, 999999).ToString();
                if (!_context.Tokens.Any(p => p.Id == oldTokenId))
                {
                    break;
                }
            }
            string newTokenId;
            while (true)
            {
                Random rnd = new Random();
                newTokenId = rnd.Next(100000, 999999).ToString();
                if (!_context.Tokens.Any(p => p.Id == newTokenId))
                {
                    break;
                }
            }

            //await _context.Tokens.AddAsync(new TokenEntity
            //{
            //    Id = confirmationToken,
            //    UserId = user.Id,
            //    AdditionalInfo = phoneNumber,
            //    ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["RestorePassword"]))
            //});
            //await _context.SaveChangesAsync();

            bool IsPhoneNumberAbsent = String.IsNullOrEmpty(user.PhoneNumber);

            TokenEntity oldPhoneToken = new TokenEntity
            {
                UserId = user.Id,
                Id = oldTokenId,
                ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["ConfirmEmail"])),
                AdditionalInfo = JsonConvert.SerializeObject(new ChangingContactsDTO { ContactType = ConactTypes.Old, AnotherTokenId = newTokenId, IsOldContactConfirmed = IsPhoneNumberAbsent, NewContact = phoneNumber })
            };
            TokenEntity newPhoneToken = new TokenEntity
            {
                UserId = user.Id,
                Id = newTokenId,
                ExpirationDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration.GetSection("TokenExistenceDays")["ConfirmEmail"])),
                AdditionalInfo = JsonConvert.SerializeObject(new ChangingContactsDTO { ContactType = ConactTypes.New, AnotherTokenId = oldTokenId, IsOldContactConfirmed = IsPhoneNumberAbsent, NewContact = phoneNumber })
            };
            await _context.AddAsync(oldPhoneToken);
            await _context.AddAsync(newPhoneToken);
            await _context.SaveChangesAsync();

            _logger.LogDebug($"User requested to change or add phoneNumber, left send confirmation sms: {phoneNumber}");
            try
            {
                var content = _configuration.GetSection("ChangePhoneNumberSms")[user.Language.ToString()];
                ////==== Change email to phone number
                await _smsService.SendSms(user.Email, content.Replace("#token", newTokenId + " " + phoneNumber));
                if(!IsPhoneNumberAbsent)
                {
                    await _smsService.SendSms(user.Email, content.Replace("#token", oldTokenId + " " + user.PhoneNumber));
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't send restoring sms for " + user.PhoneNumber + "\nException:\n" + ex.Message);
                response.Error = new Error(500, "Couldn't send, write for developers");
                return response;
            }
            _logger.LogDebug($"Confirmation email successfully sended: {user.Email}");

            response.Data = true;
            return response;
        }

        /*Change email to phone number*/
        public async Task<Response<bool>> ChangeOrAddPhoneConfirm(string confirmationToken)
        {
            _logger.LogDebug("Confirming new phone number");
            var response = new Response<bool>();
            var token = await _context.Tokens.Where(t => t.Id == confirmationToken)
                .Include(t => t.User).FirstOrDefaultAsync();

            if (token == null)
            {
                response.Error = new Error(400, "Token is not valid");
                return response;
            }

            if (token.User == null)
            {
                response.Error = new Error(404, "User not found");
                return response;
            }

            if (token.ExpirationDate < DateTime.UtcNow)
            {
                _context.Tokens.Remove(token);
                response.Error = new Error(400, "Confirmation date is over");
                return response;
            }            

            var additionalInfo = JsonConvert.DeserializeObject<ChangingContactsDTO>(token.AdditionalInfo);
            var anotherToken = await _context.Tokens.Where(t => t.Id == additionalInfo.AnotherTokenId).FirstOrDefaultAsync();
            var anotherAdditionalInfo = JsonConvert.DeserializeObject<ChangingContactsDTO>(anotherToken.AdditionalInfo);

            if (await _context.Users.AnyAsync(u => u.PhoneNumber == additionalInfo.NewContact))
            {
                _logger.LogDebug($"User with such phone already exists: {additionalInfo.NewContact}");
                response.Error = new Error(400, "This phone number is already used");
                return response;
            }

            switch (additionalInfo.ContactType)
            {
                case ConactTypes.New:
                    additionalInfo.IsNewContactConfirmed = true;
                    anotherAdditionalInfo.IsNewContactConfirmed = true;
                    break;
                case ConactTypes.Old:
                    additionalInfo.IsOldContactConfirmed = true;
                    anotherAdditionalInfo.IsOldContactConfirmed = true;
                    break;
            }

            token.AdditionalInfo = JsonConvert.SerializeObject(additionalInfo);
            anotherToken.AdditionalInfo = JsonConvert.SerializeObject(anotherAdditionalInfo);
            await _context.SaveChangesAsync();

            if (!additionalInfo.IsOldContactConfirmed)
            {
                response.Error = new Error(202, "Left to confirm old email");
                return response;
            }
            else if (!additionalInfo.IsNewContactConfirmed)
            {
                response.Error = new Error(202, "Left to confirm new email");
                return response;
            }


            token.User.PhoneNumber = additionalInfo.NewContact;
            token.User.IsPhoneNumberConfirmed = true;
            _context.Tokens.Remove(token);
            _context.Tokens.Remove(anotherToken);
            await _context.SaveChangesAsync();
            response.Data = true;

            _logger.LogDebug($"User confirmed phone number: {token.User.PhoneNumber}");
            try
            {
                ////==== Change email to phone number
                var content = _configuration.GetSection("PhoneNumberConfirmedSms")[token.User.Language.ToString()];
                await _smsService.SendSms(token.User.Email, content);
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't send that phone confirmed for " + token.User.PhoneNumber + "\nException:\n" + ex.Message);
                return response;
            }
            _logger.LogDebug($"phone confirmed successfully sended: {token.User.PhoneNumber}");

            return response;
        }
    }
     
}
