using CustomTenants.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace CustomTenants.CustomAttributes
{

    [AttributeUsage(AttributeTargets.All)]
    public class HostTenantAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var host = context.HttpContext.Request.Host.ToString();
            int tenantId = TenantService.GetTenantId(host);

            context.RouteData.Values.Add("tenantId", tenantId);
            context.RouteData.Values.Add("tenant", host);

            base.OnActionExecuting(context);
        }

        
    }
}
