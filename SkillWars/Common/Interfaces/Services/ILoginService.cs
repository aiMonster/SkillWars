using Common.DTO.Account;
using Common.DTO.Communication;
using Common.DTO.Login;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface ILoginService
    {
        Task<Response<string>> Register(RegistrationDTO request);        
        Task<Response<UserProfile>> ConfirmEmail(string confirmationToken);
        Task<TokenResponse> GetToken(ClaimsIdentity identity);
        Task<Response<ClaimsIdentity>> GetIdentity(string login, string password);
        Task<Response<ClaimsIdentity>> GetIdentity(string steamId);
        Task<Response<string>> RestorePasswordByEmail(string email);
        Task<Response<string>> RestorePasswordByEmailConfirm(RestorePasswordRequest request);

        Task<Response<string>> RestorePasswordByPhone(string phoneNumber);
        Task<Response<string>> RestorePasswordByPhoneConfirm(RestorePasswordRequest request);

        //for testing only
        Task<List<UserProfile>> GetAllUsers();
        Task<Response<UserProfile>> RemoveUserById(int userId);
    }
}
