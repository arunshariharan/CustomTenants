using CustomTenants.CustomAttributes;
using CustomTenants.Models;
using Microsoft.AspNetCore.Http;
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
    public class AuthenticationController : Controller
    {
        private ILogger<AuthenticationController> _logger;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
        }

        [HttpPost("signup")]
        public IActionResult CreateUser([FromBody] User user)
        {
            var tenant = RouteData.Values.SingleOrDefault(r => r.Key == "tenant").ToString();
            return Ok(tenant);
        }
    }
}
