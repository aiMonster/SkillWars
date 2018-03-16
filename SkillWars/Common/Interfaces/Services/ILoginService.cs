﻿using Common.DTO.Account;
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
        Task<Response<string>> SteamRegister(SteamRegistrationDTO request);
        Task<Response<UserInfo>> ConfirmEmail(string confirmationToken);
        Task<TokenResponse> GetToken(ClaimsIdentity identity);
        Task<Response<ClaimsIdentity>> GetIdentity(string login, string password);
        Task<Response<ClaimsIdentity>> GetIdentity(string steamId);
        Task<Response<string>> RestorePassword(string email);
        Task<Response<string>> RestorePasswordConfirm(RestorePasswordRequest request);

        //for testing only
        Task<List<UserInfo>> GetAllUsers();
        Task<Response<UserInfo>> RemoveUserById(int userId);
    }
}
