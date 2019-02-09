using CustomTenants.CustomAttributes;
using CustomTenants.Datastores;
using CustomTenants.Models;
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
        private string currentTenantHost { get; set; }

        public UserController(ILogger<UserController> logger) 
        {
            _logger = logger;
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            int tenantId = TenantService.TenantId;

            var usersResult = UserDatastore.Current.Users.Where(u => u.ActiveTenantIds.Contains(tenantId));

            if (usersResult == null) return NotFound();

            return Ok(usersResult);
        }

        [HttpGet("users/{userId}")]
        public IActionResult GetUser(int userId)
        {
            var user = UserDatastore.Current.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(user);
        }
    }
}
