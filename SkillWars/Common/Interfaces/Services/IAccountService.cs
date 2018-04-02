using Common.DTO.Account;
using Common.DTO.Communication;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface IAccountService
    {
        Task<UserProfile> GetUserProfileAsync(int userId);
        Task<Response<bool>> ChangeNickNameAsync(string nickName, int userId);
        Task<Response<bool>> ChangeLanguageAsync(Languages language, int userId);
        Task<Response<bool>> ChangeSteamIdAsync(string steamId, int userId);
        Task<Response<bool>> ChangePasswordAsync(ChangePasswordRequest request, int userId);
        Task<Response<bool>> ChangeEmailAsync(string email, int userId);
        Task<Response<bool>> ChangeEmailConfirmAsync(string confirmaitonToken);
        Task<Response<bool>> ChangeOrAddPhoneAsync(string phoneNumber, int userId);
        Task<Response<bool>> ChangeOrAddPhoneConfirmAsync(string confirmationToken);
    }
}
