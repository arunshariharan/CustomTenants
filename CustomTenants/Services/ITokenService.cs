using CustomTenants.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Services
{
    public interface ITokenService
    {
        JwtSecurityToken GenerateNewToken(User user);
    }
}
