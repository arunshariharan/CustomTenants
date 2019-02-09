using AutoMapper;
using CustomTenants.CustomAttributes;
using CustomTenants.Datastores;
using CustomTenants.Formatters;
using CustomTenants.Mappings;
using CustomTenants.Models;
using CustomTenants.Repositories;
using CustomTenants.Services;
using CustomTenants.Validations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Controllers
{
    [HostTenant]
    [Route("api")]
    public class UserController : Controller 
    {
        private ILogger<UserController> _logger;
        private IUserRepository _repository;

        public UserController(ILogger<UserController> logger, IUserRepository repository) 
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet("users/{userId}", Name = "User At Id")]
        public IActionResult GetUser(int userId)
        {
            var user = _repository.GetUser(userId);
            if (user == null) return NotFound();

            var mappedUser = UserMappings.StripSensitiveDataSingleUser(user);
            return Ok(mappedUser);
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

            User newUser = _repository.CreateUser(user);

            var mappedNewUser = UserMappings.StripSensitiveDataSingleUser(newUser);
            return CreatedAtRoute("User At Id", new { userId = mappedNewUser.Id }, mappedNewUser);
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var usersResult = _repository.GetUsers();
            if (usersResult == null) return NotFound();

            var mappedUsers = UserMappings.StripSensitiveDataMultipleUsers(usersResult);

            return Ok(mappedUsers);
        }

        [HttpPost("users/{userId}/makeAdmin")]
        public IActionResult MakeUserAdmin(int userId)
        {
            var user = _repository.GetUser(userId);
            if (user == null) return NotFound();

            _repository.MakeAdmin(user);

            return Ok();
        }

        [HttpPost("users/{userId}/removeAdmin")]
        public IActionResult RemoveUserFromAdmin(int userId)
        {
            var user = _repository.GetUser(userId);
            if (user == null) return NotFound();

            _repository.RemoveAdmin(user);

            return Ok();
        }
    }
}
