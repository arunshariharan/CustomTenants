using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MultiTenant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiTenant.Api.CustomAttributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class ClaimsAttribute : TypeFilterAttribute
    {
        public ClaimsAttribute(string claimType) : base(typeof(ClaimAccessFilter))
        {
            Arguments = new object[] { claimType };
        }
    }
    
    public class ClaimAccessFilter : IAuthorizationFilter
    {
        readonly string _type;
        public ClaimAccessFilter(string type)
        {
            _type = type;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            var tenantHost = context.HttpContext.Request.Host.ToString();

            try
            {
                int tenantId = TenantService.GetCurrentTenantId(tenantHost);
                TenantService.SetTenantDetails(tenantHost);

                if (!context.RouteData.Values.Any(a => a.Key == "tenantId"))
                    context.RouteData.Values.Add("tenantId", tenantId);
                if (!context.RouteData.Values.Any(a => a.Key == "tenant"))
                    context.RouteData.Values.Add("tenant", tenantHost);
                if (!context.RouteData.Values.Any(a => a.Key == "tenantName"))
                    context.RouteData.Values.Add("tenantName", TenantService.TenantName);

                var claimType = context.HttpContext.User.Claims.FirstOrDefault(u => u.Type == _type).Value;
                if(claimType != TenantService.TenantId.ToString())
                {
                    context.Result = new ForbidResult();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Current Host does not have permissions to access this resource. Host: ${tenantHost}. ${e}");
            }
        }
    }
}
