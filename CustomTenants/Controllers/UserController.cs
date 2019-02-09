using CustomTenants.CustomAttributes;
using CustomTenants.Datastores;
using CustomTenants.Models;
using CustomTenants.Repositories;
using CustomTenants.Services;
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

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var usersResult = _repository.GetUsers();
            if (usersResult == null) return NotFound();

            return Ok(usersResult);
        }

        [HttpGet("users/{userId}")]
        public IActionResult GetUser(int userId)
        {
            var user = _repository.GetUser(userId);
            if (user == null) return NotFound();

            return Ok(user);
        }
    }
}
