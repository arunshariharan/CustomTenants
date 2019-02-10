using CustomTenants.CustomAttributes;
using CustomTenants.Formatters;
using CustomTenants.Mappings;
using CustomTenants.Models;
using CustomTenants.Repositories;
using CustomTenants.Services;
using CustomTenants.Validations;
using Microsoft.AspNetCore.Authorization;
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
        private ITokenManager _tokenManager;

        public AuthenticationController(ILogger<AuthenticationController> logger, 
            IUserRepository repository, 
            IUserMappings userMappings,
            IConfiguration configuration,
            ITokenManager tokenManager)
        {
            _logger = logger;
            _repository = repository;
            _userMappings = userMappings;
            _configuration = configuration;
            _tokenManager = tokenManager;
        }

        /// <summary></summary>
        /// <param name="userCreds"> Takes in email and password</param>
        /// <returns></returns>
        /// <response code="200">Returns JWT Token + Expiry that needs to be attached with every other request for 
        /// Authentication and Authorization</response>
        /// <response code="400">The payload is malformed</response>
        /// <response code="404">Provided email address is not found in DB</response>
        /// <response code="401">Email / Password do not match</response>

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

                var token = _tokenManager.GenerateNewToken(user);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), expiration = token.ValidTo });

            } catch (Exception e)
            {
                _logger.LogError($"Could not create JWT. Error: ${e}");
            }

            return BadRequest("Failed to signin. Coud not generate token");

        }

        /// <summary></summary>
        /// <param name="user">Must contain FullName, email and password at the minimum</param>
        /// <returns></returns>
        /// <response code="201">Returns the new user details in body and location in header</response>
        /// <response code="400">Email already exists / Malformed payload</response>
        /// <response code="500">Unable to create new user in DB</response>
        
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

            try
            {
                User newUser = _repository.CreateUser(user);
                var mappedNewUser = _userMappings.StripSensitiveDataSingleUser(newUser);
                return CreatedAtRoute("User At Id", new { userId = mappedNewUser.Id }, mappedNewUser);
            } catch(Exception e)
            {
                _logger.LogError($"Something went wrong while trying to create user. ${e}");
            }

            return StatusCode(500, "Unable to create new user. Try again later");            
        }


        /// <summary></summary>
        /// <param name="updatedPassword">Takes in Old password and new password</param>
        /// <returns></returns>
        /// <response code="200">Password successfully updated</response>
        /// <response code="400">The payload is malformed</response>
        /// <response code="404">Logged in user details could not be verified</response>
        /// <response code="401">Not authorized to perform this operation. User not signed in</response>
        /// <response code="500">Unable to update password</response>

        [Authorize]
        [HttpPost("updatePassword")]
        public IActionResult UpdatePassword([FromBody] PasswordUpdate updatedPassword)
        {
            PasswordUpdateValidator validator = new PasswordUpdateValidator();
            var results = validator.Validate(updatedPassword);

            var errors = results.ToString("\n");
            if (errors != string.Empty)
            {
                var errorList = ErrorFormatter.FormatValidationErrors(errors);
                return BadRequest(new { Errors = errorList });
            }

            User currentUser = _repository.GetUser(User.Claims);
            if (currentUser == null) return NotFound("User could not be validated or not found for this operation");

            bool isOldPasswordMatching = _repository.ValidatePassword(updatedPassword.OldPassword, currentUser);
            if (!isOldPasswordMatching) return BadRequest("The Old password enetered does not match with the user's exisisting password.");

            try
            {
                _repository.UpdatePassword(updatedPassword.NewPassword, currentUser);
                return Ok("Password updated");

            } catch(Exception e)
            {
                _logger.LogError($"Something went wrong while trying to update password. Password not updated. ${e}");
            }

            return StatusCode(500, "Unable to update Password. Try again later");
        }
    }
}
