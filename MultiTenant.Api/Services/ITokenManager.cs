﻿using MultiTenant.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiTenant.Services
{
    public interface ITokenManager
    {
        JwtSecurityToken GenerateNewToken(User user);
        bool ValidateClaimHasType(IEnumerable<Claim> claim, string typeToCheck);
    }
}
