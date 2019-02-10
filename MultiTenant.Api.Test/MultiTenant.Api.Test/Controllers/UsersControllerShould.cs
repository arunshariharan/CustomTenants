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

namespace MultiTenant.Api.Test.Controllers
{
    public class UsersControllerShould
    {
        private Mock<IConfiguration> _mockConfig;
        private Mock<IUserRepository> _repository;
        private Mock<ITokenManager> _tokenManager;
        private Mock<IUserMappings> _userMapping;
        private Mock<ILogger<UsersController>> _logger;
        private readonly UsersController _usersController;
        private User _user;
        private UserWithoutSensitiveDataDto _userWithoutSensitiveData;
        private UserContact _userContact;

        public UsersControllerShould()
        {
            _mockConfig = new Mock<IConfiguration>();
            _repository = new Mock<IUserRepository>();
            _tokenManager = new Mock<ITokenManager>();
            _logger = new Mock<ILogger<UsersController>>();
            _userMapping = new Mock<IUserMappings>();
            _userContact = new UserContact() { EmailAddress = "test@email.address" };
            _user = new User()
            {
                Id = 1,
                FullName = "User 1",
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

            _usersController = new UsersController(_logger.Object, 
                _repository.Object, 
                _userMapping.Object, 
                _tokenManager.Object);
            
        }

        [Fact]
        public void ReturnListOfUsers()
        {
            _repository.Setup(r => r.GetUsers()).Returns(new[] { _user } );
            _userMapping.Setup(u => u.StripSensitiveDataMultipleUsers(new[] { _user }))
                .Returns(new List<UserWithoutSensitiveDataDto>() { _userWithoutSensitiveData });

            IActionResult result = _usersController.GetUsers();
            var okResult = result as OkObjectResult;
            
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void ReturnUserAtId()
        {
            int userId = 1;
            _repository.Setup(r => r.GetUser(userId)).Returns(_user);
            _userMapping.Setup(u => u.StripSensitiveDataSingleUser( _user ))
                .Returns( _userWithoutSensitiveData );

            IActionResult result = _usersController.GetUser(userId);
            var okResult = result as OkObjectResult;

            Assert.NotNull(okResult.Value);
            Assert.Equal(okResult.Value, _userWithoutSensitiveData);            
        }

        [Fact]
        public void Return404ForNonExisistentUser_GetUserAtId()
        {
            int userId = 2;
            _repository.Setup(r => r.GetUser(userId)).Returns((User)null);

            IActionResult result = _usersController.GetUser(userId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void MakeAdminIfUserIsAuthorized()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(true);            
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.MakeAdmin(_user));
            
            IActionResult result = _usersController.MakeUserAdmin(_userContact);

            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public void ReturnUnauthorizedIfUserNotSignedinCurrentTenant()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(false);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.MakeUserAdmin(_userContact);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Return404IfUserIsNotFound_MakeAdmin()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1"),
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(true);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns((User)null);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.MakeUserAdmin(_userContact);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void RemoveAdminIfUserIsAuthorized()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1"),
              new Claim("Email", "Not@matching.email")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(true);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.RemoveUserFromAdmin(_userContact);

            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public void ReturnUnauthorizedIfUserNotSignedinCurrentTenant_RemoveAdmin()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(false);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.RemoveUserFromAdmin(_userContact);

            Assert.IsType<UnauthorizedResult>(result);
        }

        
        [Fact]
        public void Return404IfUserIsNotFound_RemoveAdmin()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(true);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns((User)null);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.RemoveUserFromAdmin(_userContact);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeactivateIfUserIsAuthorized()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1"),
              new Claim("Email", _userContact.EmailAddress)
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(true);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.DeactivateUser(_userContact);

            Assert.IsType<OkObjectResult>(result);
        }


        [Fact]
        public void ReturnUnauthorizedIfUserNotSignedinCurrentTenant_Deactivate()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(false);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.DeactivateUser(_userContact);

            Assert.IsType<UnauthorizedResult>(result);
        }
        

        [Fact]
        public void Return404IfUserIsNotFound_Deactivate()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(true);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns((User)null);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.DeactivateUser(_userContact);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void ActivateIfUserIsAuthorized()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1"),
              new Claim("Email", _userContact.EmailAddress)
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(true);
            _repository.Setup(r => r.GetDeactivatedUser(_userContact.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.ActivateUser(_userContact);

            Assert.IsType<OkObjectResult>(result);
        }


        [Fact]
        public void ReturnUnauthorizedIfUserNotSignedinCurrentTenant_Activate()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(false);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns(_user);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.ActivateUser(_userContact);

            Assert.IsType<UnauthorizedResult>(result);
        }
        

        [Fact]
        public void Return404IfUserIsNotFound_Activate()
        {
            Claim[] claim = new[]
            {
              new Claim("TokenIssuedForCurrentTenant", "1")
            };

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.Claims).Returns(claim);
            _usersController.ControllerContext.HttpContext = httpContext.Object;

            _tokenManager.Setup(t => t.ValidateClaimHasType(claim, "TokenIssuedForCurrentTenant")).Returns(true);
            _repository.Setup(r => r.GetUser(_userContact.EmailAddress)).Returns((User)null);
            _repository.Setup(r => r.MakeAdmin(_user));

            IActionResult result = _usersController.ActivateUser(_userContact);

            Assert.IsType<NotFoundResult>(result);
        }

    }
}
