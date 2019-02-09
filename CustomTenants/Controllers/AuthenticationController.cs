using CustomTenants.CustomAttributes;
using CustomTenants.Formatters;
using CustomTenants.Mappings;
using CustomTenants.Models;
using CustomTenants.Repositories;
using CustomTenants.Services;
using CustomTenants.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CustomTenants.Controllers
{
    [HostTenant]
    [Route("api")]
    public class AuthenticationController : Controller
    {
        private ILogger<AuthenticationController> _logger;
        private IUserRepository _repository;
        private IUserMappings _userMappings;
        private IConfiguration _configuration;
        private ITokenService _tokenService;

        public AuthenticationController(ILogger<AuthenticationController> logger, 
            IUserRepository repository, 
            IUserMappings userMappings,
            IConfiguration configuration,
            ITokenService tokenService)
        {
            _logger = logger;
            _repository = repository;
            _userMappings = userMappings;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("signin")]
        public IActionResult SignInWithToken([FromBody] UserCredentials userCreds)
        {
            _logger.LogInformation(userCreds.EmailAddress);
            UserCredentialsValidator validator = new UserCredentialsValidator();
            var results = validator.Validate(userCreds);

            var errors = results.ToString("\n");
            if (errors != string.Empty)
            {
                var errorList = ErrorFormatter.FormatValidationErrors(errors);
                return BadRequest(new { Errors = errorList });
            }

            try
            {
                // By default we return only if the user has curretn tenant in active tenants. 
                // If the user is not active in current tenant, we will return not found
                User user = _repository.GetUser(userCreds.EmailAddress);
                if (user == null) return NotFound("User not found or has not access");


                bool isValidUser = _repository.ValidatePassword(userCreds, user);
                if (!isValidUser) return Unauthorized();

                var token = _tokenService.GenerateNewToken(user);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), expiration = token.ValidTo });

            } catch (Exception e)
            {
                _logger.LogError($"Could not create JWT. Error: ${e}");
            }

            return BadRequest("Failed to signin. Coud not generate token");

        }

        [HttpPost("newUser")]
        public IActionResult CreateUser([FromBody] User user)
        {
            UserValidator validator = new UserValidator();
            var results = validator.Validate(user);

            var errors = results.ToString("\n");
            if (errors != string.Empty)
            {
                var errorList = ErrorFormatter.FormatValidationErrors(errors);
                return BadRequest(new { Errors = errorList });
            }

            if(_repository.GetUser(user.EmailAddress) != null)
            {
                return BadRequest("Email address already exists. Please sign in");
            }

            User newUser = _repository.CreateUser(user);

            var mappedNewUser = _userMappings.StripSensitiveDataSingleUser(newUser);
            return CreatedAtRoute("User At Id", new { userId = mappedNewUser.Id }, mappedNewUser);
        }
    }
}
