using CustomTenants.CustomAttributes;
using CustomTenants.Formatters;
using CustomTenants.Mappings;
using CustomTenants.Models;
using CustomTenants.Repositories;
using CustomTenants.Validations;
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
        private IUserRepository _repository;
        private IUserMappings _userMappings;

        public AuthenticationController(ILogger<AuthenticationController> logger, IUserRepository repository, IUserMappings userMappings)
        {
            _logger = logger;
            _repository = repository;
            _userMappings = userMappings;
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

            var mappedNewUser = _userMappings.StripSensitiveDataSingleUser(newUser);
            return CreatedAtRoute("User At Id", new { userId = mappedNewUser.Id }, mappedNewUser);
        }
    }
}
