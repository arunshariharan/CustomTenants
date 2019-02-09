using CustomTenants.CustomAttributes;
using CustomTenants.Formatters;
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

        public AuthenticationController(ILogger<AuthenticationController> logger, IUserRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }
    }
}
