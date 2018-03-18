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
        Task<UserProfile> GetUserProfile(int userId);
        Task<Response<bool>> ChangeNickName(string nickName, int userId);
        Task<Response<bool>> ChangeLanguage(Languages language, int userId);
        Task<Response<bool>> ChangeSteamId(string steamId, int userId);
        Task<Response<bool>> ChangePassword(ChangePasswordRequest request, int userId);
    }
}
