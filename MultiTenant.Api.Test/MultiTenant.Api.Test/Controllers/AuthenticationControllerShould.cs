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
using System.Collections.Generic;
using MultiTenant.Mappings;
using Microsoft.Extensions.Logging;
using MultiTenant.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace MultiTenant.Api.Test.Controllers
{
    public class AuthenticationControllerShould
    {
        private Mock<IConfiguration> _mockConfig;
        private Mock<IUserRepository> _repository;
        private Mock<ITokenManager> _tokenManager;
        private Mock<IUserMappings> _userMapping;
        private Mock<ILogger<AuthenticationController>> _logger;
        private readonly AuthenticationController _authController;
        private User _user;
        private UserCredentials _userCreds;
        private PasswordUpdate _updatedPassword;
        private UserWithoutSensitiveDataDto _userWithoutSensitiveData;

        public AuthenticationControllerShould()
        {
            _mockConfig = new Mock<IConfiguration>();
            _repository = new Mock<IUserRepository>();
            _tokenManager = new Mock<ITokenManager>();
            _logger = new Mock<ILogger<AuthenticationController>>();
            _userMapping = new Mock<IUserMappings>();
            _updatedPassword = new PasswordUpdate() { OldPassword = "12345678", NewPassword = "abcdefgh123" };
            _user = new User()
            {
                FullName = "User 1",
                EmailAddress = "email1@email.com",
                Password = "SuperSecure"
            };

            _userCreds = new UserCredentials()
            {
                EmailAddress = "email1@email.com",
                Password = "SuperSecure"
            };

            _userWithoutSensitiveData = new UserWithoutSensitiveDataDto()
            {
                Id = 1,
                FullName = "User 1",
                EmailAddress = "email1@email.com",
                ActiveTenantIds = { 1 },
                AdminForTenants = { 1 }
            };

            _authController = new AuthenticationController(_logger.Object,
                _repository.Object,
                _userMapping.Object,
                _mockConfig.Object,
                _tokenManager.Object);
        }

        [Fact]
        public void SigninUserWithValidCreds()
        {
            _repository.Setup(r => r.GetUser(_userCreds.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.ValidatePassword(_userCreds, _user)).Returns(true);
            _tokenManager.Setup(t => t.GenerateNewToken(_user)).Returns(new JwtSecurityToken());

            IActionResult signinResult = _authController.SignInWithToken(_userCreds);
            var okSigninResultObject = signinResult as OkObjectResult;

            Assert.NotNull(okSigninResultObject.Value);
        }

        [Fact]
        public void ReturnUnauthorizedIfUserCredsDontMatch_Signin()
        {
            _repository.Setup(r => r.GetUser(_userCreds.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.ValidatePassword(_userCreds, _user)).Returns(false);

            IActionResult signinResult = _authController.SignInWithToken(_userCreds);
            
            Assert.IsType<UnauthorizedResult>(signinResult);
        }

        [Fact]
        public void ReturnBadRequestIfPayloadIsNotValid_Signin()
        {
            IActionResult signinResult = _authController.SignInWithToken(new UserCredentials { EmailAddress = "234" });

            Assert.IsType<BadRequestObjectResult>(signinResult);
        }

        [Fact]
        public void ReturnNotFoundIfUserDoesNotExist_Signin()
        {
            _repository.Setup(r => r.GetUser(_userCreds.EmailAddress)).Returns((User)null);

            IActionResult signinResult = _authController.SignInWithToken(_userCreds);

            Assert.IsType<NotFoundObjectResult>(signinResult);
        }

        [Fact]
        public void CreateNewUserWithValidData()
        {
            _repository.Setup(r => r.GetUser(_user.EmailAddress)).Returns((User)null);
            _repository.Setup(r => r.CreateUser(_user)).Returns(_user);
            _userMapping.Setup(u => u.StripSensitiveDataSingleUser(_user)).Returns(_userWithoutSensitiveData);

            IActionResult newUserResult = _authController.CreateUser(_user);
            var newUserResultCreatedAtRoute = newUserResult as CreatedAtRouteResult;
            Assert.NotNull(newUserResultCreatedAtRoute.Value);
        }

        [Fact]
        public void ReturnBadRequestIfUserAlreadyExists()
        {
            _repository.Setup(r => r.GetUser(_user.EmailAddress)).Returns(_user);

            IActionResult newUserResult = _authController.CreateUser(_user);

            Assert.IsType<BadRequestObjectResult>(newUserResult);
        }

        [Fact]
        public void ReturnBadRequestIfPayloadIsMalformed()
        {
            IActionResult newUserResult = _authController.CreateUser(new User { FullName = "Incomplete User Model" });

            Assert.IsType<BadRequestObjectResult>(newUserResult);
        }
        
        [Fact]
        public void UpdatePasswordWithValidData()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _authController.ControllerContext.HttpContext = httpContext.Object;

            _repository.Setup(r => r.GetUser(claim)).Returns(_user);
            _repository.Setup(r => r.ValidatePassword(_updatedPassword.OldPassword, _user)).Returns(true);

            IActionResult updatePasswordResult = _authController.UpdatePassword(_updatedPassword);
            Assert.IsType<OkObjectResult>(updatePasswordResult);
        }

        [Fact]
        public void ReturnBadRequestIfOldPasswordDoesNotMatch()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _authController.ControllerContext.HttpContext = httpContext.Object;

            _repository.Setup(r => r.GetUser(claim)).Returns(_user);
            _repository.Setup(r => r.ValidatePassword(_updatedPassword.OldPassword, _user)).Returns(false);

            IActionResult updatePasswordResult = _authController.UpdatePassword(_updatedPassword);
            Assert.IsType<BadRequestObjectResult>(updatePasswordResult);
        }

        [Fact]
        public void ReturnBadRequestIfPayloadIsMalformed_UpdatePassword()
        {
            IActionResult newUserResult = _authController.UpdatePassword(new PasswordUpdate { OldPassword = "Incomplete Password Model" });

            Assert.IsType<BadRequestObjectResult>(newUserResult);
        }

        
    }
}
