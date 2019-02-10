using Moq;
using MultiTenant.Models;
using MultiTenant.Services;
using System.IdentityModel.Tokens.Jwt;
using System;
using Xunit;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MultiTenant.Repositories;
using System.Security.Claims;

namespace MultiTenant.Api.Test.Services
{
    public class TokenManagerShould
    {
        private JwtTokenManager _tokenManager;
        private Mock<IConfiguration> _mockConfig;
        private Mock<IUserRepository> _repository;
        
        public TokenManagerShould()
        {
            _mockConfig = new Mock<IConfiguration>();
            _repository = new Mock<IUserRepository>();
            _tokenManager = new JwtTokenManager(_mockConfig.Object, _repository.Object);
        }

        [Fact]
        public void ReturnTokenWithPassedInUserDetails_GenerateToken()
        {
            User user = new User()
            {
                FullName = "Mock User",
                EmailAddress = "mock@test.user"
            };

            
            var result = _tokenManager.GenerateNewToken(user);
            var claimsEmail = result.Claims.FirstOrDefault(x => x.Type == "Email").Value.ToString();
            var claimsName = result.Claims.FirstOrDefault(x => x.Type == "given_name").Value.ToString();
            
            Assert.IsType<JwtSecurityToken>(result);
            Assert.Equal(claimsEmail, user.EmailAddress);
            Assert.Equal(claimsName, user.FullName);            
        }

        [Fact]
        public void ReturnTokenWithAdminValueAsFalse_GenerateToken()
        {
            User user = new User()
            {
                FullName = "Mock User",
                EmailAddress = "mock@test.user"
            };

            _repository.Setup(r => r.IsAdmin(user)).Returns(false);

            var result = _tokenManager.GenerateNewToken(user);
            var isAdmin = Convert.ToBoolean(result.Claims.FirstOrDefault(x => x.Type == "Admin").Value);
            
            Assert.False(isAdmin);
        }

        [Fact]
        public void ReturnTokenWithAdminValueAsTrue_GenerateToken()
        {
            User user = new User()
            {
                FullName = "Mock User",
                EmailAddress = "mock@test.user"
            };

            _repository.Setup(r => r.IsAdmin(user)).Returns(true);

            var result = _tokenManager.GenerateNewToken(user);
            var isAdmin = Convert.ToBoolean(result.Claims.FirstOrDefault(x => x.Type == "Admin").Value);
            
            Assert.True(isAdmin);
        }

        [Theory]
        [InlineData("TokenIssuedForCurrentTenant", "1", 1, true)]
        [InlineData("TokenIssuedForCurrentTenant", "1", 2, false)]
        [InlineData("DoesnotExist", "1", 1, false)]
        public void ReturnTrueWhenClaimIncludesType_ValidateClaimHasType(string claimName, string claimValue, int tenantId, bool expectedResult)
        {
            TenantService.TenantId = tenantId;
            Claim[] claim = new[] 
            {
              new Claim(claimName, claimValue),
              new Claim("DummyValue", "This is dummy")
            };
            
            bool result = _tokenManager.ValidateClaimHasType(claim, claimName);

            Assert.Equal(result, expectedResult);
        }

    }
}
