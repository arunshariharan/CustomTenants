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
    [Route("api/users")]
    public class UsersController : Controller 
    {
        private ILogger<UsersController> _logger;
        private IUserRepository _repository;
        private IUserMappings _userMappings;

        public UsersController(ILogger<UsersController> logger, IUserRepository repository, IUserMappings userMappings) 
        {
            _logger = logger;
            _repository = repository;
            _userMappings = userMappings;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
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

        [HttpPost("{userId}/makeAdmin")]
        public IActionResult MakeUserAdmin(int userId)
        {
            var user = _repository.GetUser(userId);
            if (user == null) return NotFound();

            _repository.MakeAdmin(user);
            return Ok();
        }

        [HttpPost("{userId}/removeAdmin")]
        public IActionResult RemoveUserFromAdmin(int userId)
        {
            var user = _repository.GetUser(userId);
            if (user == null) return NotFound();

            _repository.RemoveAdmin(user);
            return Ok();
        }
    }
}
