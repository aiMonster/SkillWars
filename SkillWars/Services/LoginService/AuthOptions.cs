using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.LoginService
{
    public class AuthOptions
    {
        public const string ISSUER = "skill_wars_web_api_server";
        public const string AUDIENCE = "skill_wars_web_api_users";
        const string KEY = "security_80_lvl.ty_ne_proydesh";
        public const int LIFETIME = 60 * 24;
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
