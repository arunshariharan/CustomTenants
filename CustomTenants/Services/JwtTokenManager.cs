using CustomTenants.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CustomTenants.Services
{
    public class JwtTokenManager : ITokenManager
    {
        private IConfiguration _configuration;
        public JwtTokenManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public JwtSecurityToken GenerateNewToken(User user)
        {
            var claims = GenerateNewClaim(user);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisExtraLogSecret1sForMultiTenantAPIC0d1ngChall3nge"));
            var signingCreds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: TenantService.TenantName,
                audience: TenantService.TenantHost,
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: signingCreds
                );

            return token;
        }

        private Claim[] GenerateNewClaim(User user)
        {
            bool isAdmin = user.AdminForTenants.Contains(TenantService.TenantId);

            return new[]
               {
                    new Claim(JwtRegisteredClaimNames.Sub, user.EmailAddress),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.GivenName, user.FullName),
                    new Claim("Admin", isAdmin.ToString()),
                    new Claim("TokenIssuedForCurrentTenant", TenantService.TenantId.ToString()),
                    new Claim("Email", user.EmailAddress)
                };
        }

        public bool ValidateClaimHasType(IEnumerable<Claim> claim, string typeToCheck)
        {
            var jwtTokenIssuer = claim.FirstOrDefault(u => u.Type == typeToCheck).Value;
            if (typeToCheck == "TokenIssuedForCurrentTenant")
                return (jwtTokenIssuer == TenantService.TenantId.ToString());            

            return false;
        }
    }
}
