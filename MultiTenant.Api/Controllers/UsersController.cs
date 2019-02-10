using AutoMapper;
using MultiTenant.CustomAttributes;
using MultiTenant.Datastores;
using MultiTenant.Formatters;
using MultiTenant.Mappings;
using MultiTenant.Models;
using MultiTenant.Repositories;
using MultiTenant.Services;
using MultiTenant.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenant.Controllers
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

        /// <summary>Returns all registered user in this Tenant. 
        /// Change to different tenant in swagger URL to get different results</summary>
        /// <remarks>Must be signed in to access this endpoint</remarks>
        /// <response code="200">List of all Users active in this tenant</response>
        /// <response code="401">User not signed in to perform this request</response>
        
        [HttpGet]
        public IActionResult GetUsers()
        {
            var usersResult = _repository.GetUsers();
            if (usersResult == null) return NotFound();

            var mappedUsers = _userMappings.StripSensitiveDataMultipleUsers(usersResult);
            return Ok(mappedUsers);
        }

        /// <summary>
        /// Get a user on particular Id
        /// </summary>
        /// <param name="userId">Integer</param>
        /// <remarks>Must be signed in to access this endpoint</remarks>
        /// <response code="200">List of all Users active in this tenant</response>
        /// <response code="401">User not signed in to perform this request</response>
        /// <response code="404">User not found with given ID</response>
        
        [HttpGet("{userId}", Name = "User At Id")]
        public IActionResult GetUser(int userId)
        {
            var user = _repository.GetUser(userId);
            if (user == null) return NotFound();

            var mappedUser = _userMappings.StripSensitiveDataSingleUser(user);
            return Ok(mappedUser);
        }

        /// <summary>
        /// Make a user in current Tenant as an admin
        /// </summary>
        /// <param name="userContact">Currently takes in only email address</param>
        /// <response code="401">User not signed in to perform this request. 
        /// Or the User is not signed into the current tenant. User must obtain a token from current tenant to perform this operation</response>
        /// <response code="403">User does not have admin privileges to perform this action</response>
        /// <response code="404">User with supplied contact cannot be found in db</response>
        /// <response code="200">Success</response>
        /// <response code="500">Error while writing to data store</response>

        [HttpPost("makeAdmin")]
        [Authorize(Policy = "Admin")]
        public IActionResult MakeUserAdmin([FromBody] UserContact userContact)
        {
            var isValidForcurrentTenant = _tokenManager.ValidateClaimHasType(User.Claims, "TokenIssuedForCurrentTenant");
            if (!isValidForcurrentTenant)
                return Unauthorized();

            var user = _repository.GetUser(userContact.EmailAddress);
            if (user == null) return NotFound();

            try
            {
                _repository.MakeAdmin(user);
                return Ok();
            } catch (Exception e)
            {
                _logger.LogError($"Error occured while trynig to make user Admin. ${e}");
            }

            return StatusCode(500, "Something went wrong while trying to make user an admin.");
        }

        /// <summary>
        /// Remove admin privilege for a user in current tenant
        /// </summary>
        /// <param name="userContact">Currently takes in only email address</param>
        /// <response code="401">User not signed in to perform this request. 
        /// Or the User is not signed into the current tenant. User must obtain a token from current tenant to perform this operation</response>
        /// <response code="403">User does not have admin privileges to perform this action</response>
        /// <response code="404">User with supplied contact cannot be found in db</response>
        /// <response code="200">Success</response>
        /// <response code="500">Error while writing to data store</response>
        
        [HttpPost("removeAdmin")]
        [Authorize(Policy = "Admin")]
        public IActionResult RemoveUserFromAdmin([FromBody] UserContact userContact)
        {
            var isValidForcurrentTenant = _tokenManager.ValidateClaimHasType(User.Claims, "TokenIssuedForCurrentTenant");
            if (!isValidForcurrentTenant)
                return Unauthorized();

            var user = _repository.GetUser(userContact.EmailAddress);
            if (user == null) return NotFound();

            try
            {
                _repository.RemoveAdmin(user);
                return Ok();
            } catch (Exception e)
            {
                _logger.LogError($"Error occured while trynig to make user Admin. ${e}");
            }

            return StatusCode(500, "Something went wrong while trying to make user an admin.");
        }

        /// <summary>
        /// Deactivate a user from curretn tenant. The user can access other Tenants though.
        /// </summary>
        /// <param name="userContact">Currently takes in only email address</param>
        /// <response code="401">User not signed in to perform this request. 
        /// Or the User is not signed into the current tenant. User must obtain a token from current tenant to perform this operation</response>
        /// <response code="403">User does not have admin privileges to perform this action</response>
        /// <response code="404">User with supplied contact cannot be found in db</response>
        /// <response code="200">Successfully deactivated user</response>
        /// <response code="500">Error while writing to data store</response>

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
                _logger.LogError($"Exception while trying to deactivate user: ${userContact.EmailAddress}. Exception: ${e}");
            }

            return BadRequest("Unable to perform requested action");
        }

        /// <summary>
        /// Activate a user on current tenant. The user must be currently deactivated in current tenant.
        /// </summary>
        /// <param name="userContact">Currently takes in only email address</param>
        /// <response code="401">User not signed in to perform this request. 
        /// Or the User is not signed into the current tenant. User must obtain a token from current tenant to perform this operation</response>
        /// <response code="403">User does not have admin privileges to perform this action</response>
        /// <response code="404">User with supplied contact cannot be found in db</response>
        /// <response code="200">Successfully activated user</response>
        /// <response code="500">Error while writing to data store</response>
        
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
