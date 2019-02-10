using AutoMapper;
using CustomTenants.CustomAttributes;
using CustomTenants.Datastores;
using CustomTenants.Formatters;
using CustomTenants.Mappings;
using CustomTenants.Models;
using CustomTenants.Repositories;
using CustomTenants.Services;
using CustomTenants.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Controllers
{
    [Authorize]
    [HostTenant]
    [Route("api/users")]
    public class UsersController : Controller 
    {
        private ILogger<UsersController> _logger;
        private IUserRepository _repository;
        private IUserMappings _userMappings;
        private ITokenManager _tokenManager;

        public UsersController(ILogger<UsersController> logger, 
            IUserRepository repository, 
            IUserMappings userMappings,
            ITokenManager tokenManager) 
        {
            _logger = logger;
            _repository = repository;
            _userMappings = userMappings;
            _tokenManager = tokenManager;
        }

        
        [HttpGet]
        public IActionResult GetUsers()
        {
            _logger.LogInformation(User.Claims.FirstOrDefault(a => a.Type == "TokenIssuedForCurrentTenant").Value.ToString());
            var usersResult = _repository.GetUsers();
            if (usersResult == null) return NotFound();

            var mappedUsers = _userMappings.StripSensitiveDataMultipleUsers(usersResult);
            return Ok(mappedUsers);
        }

        [HttpGet("{userId}", Name = "User At Id")]
        public IActionResult GetUser(int userId)
        {
            var user = _repository.GetUser(userId);
            if (user == null) return NotFound();

            var mappedUser = _userMappings.StripSensitiveDataSingleUser(user);
            return Ok(mappedUser);
        }

        [HttpPost("makeAdmin")]
        [Authorize(Policy = "Admin")]
        public IActionResult MakeUserAdmin([FromBody] UserContact userContact)
        {
            var isValidForcurrentTenant = _tokenManager.ValidateClaimHasType(User.Claims, "TokenIssuedForCurrentTenant");
            if (!isValidForcurrentTenant)
                return Unauthorized();

            var user = _repository.GetUser(userContact.EmailAddress);
            if (user == null) return NotFound();

            _repository.MakeAdmin(user);
            return Ok();
        }

        [HttpPost("removeAdmin")]
        [Authorize(Policy = "Admin")]
        public IActionResult RemoveUserFromAdmin([FromBody] UserContact userContact)
        {
            var isValidForcurrentTenant = _tokenManager.ValidateClaimHasType(User.Claims, "TokenIssuedForCurrentTenant");
            if (!isValidForcurrentTenant)
                return Unauthorized();

            var user = _repository.GetUser(userContact.EmailAddress);
            if (user == null) return NotFound();

            _repository.RemoveAdmin(user);
            return Ok();
        }

        [HttpPost("deactivateUser")]
        [Authorize(Policy = "Admin")]
        public IActionResult DeactivateUser([FromBody] UserContact userContact)
        {
            var isValidForcurrentTenant = _tokenManager.ValidateClaimHasType(User.Claims, "TokenIssuedForCurrentTenant");
            if (!isValidForcurrentTenant)
                return Unauthorized();

            var user = _repository.GetUser(userContact.EmailAddress);
            if (user == null) return NotFound();

            // if signed in user the same as requesting for deactivate - deny request
            var signedInUser = User.Claims.FirstOrDefault(u => u.Type == "Email").Value;
            if (signedInUser == user.EmailAddress)
                return Unauthorized();

            try
            {
                _repository.DeactivateUser(user);
                return Ok("Deactivation successfull");
            } catch(Exception e)
            {
                _logger.LogError($"Exception while trying to deactivate user: ${userContact.EmailAddress}");
            }

            return BadRequest("Unable to perform requested action");
        }

        [HttpPost("activateUser")]
        [Authorize(Policy = "Admin")]
        public IActionResult ActivateUser([FromBody] UserContact userContact)
        {
            var isValidForcurrentTenant = _tokenManager.ValidateClaimHasType(User.Claims, "TokenIssuedForCurrentTenant");
            if (!isValidForcurrentTenant)
                return Unauthorized();

            var user = _repository.GetDeactivatedUser(userContact.EmailAddress);
            if (user == null) return NotFound();
            
            try
            {
                _repository.ActivateUser(user);
                return Ok("Activation successfull");
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception while trying to activate user: ${userContact.EmailAddress}");
            }

            return BadRequest("Unable to perform requested action");
        }
    }
}
