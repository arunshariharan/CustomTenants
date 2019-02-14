﻿using MultiTenant.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace MultiTenant.CustomAttributes
{

    [AttributeUsage(AttributeTargets.All)]
    public class HostTenantAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var tenantHost = context.HttpContext.Request.Host.ToString();

            try
            {
                int tenantId = TenantService.GetCurrentTenantId(tenantHost);
                TenantService.SetTenantDetails(tenantHost);

                if(!context.RouteData.Values.Any(a => a.Key == "tenantId"))
                    context.RouteData.Values.Add("tenantId", tenantId);
                if (!context.RouteData.Values.Any(a => a.Key == "tenant"))
                    context.RouteData.Values.Add("tenant", tenantHost);
                if (!context.RouteData.Values.Any(a => a.Key == "tenantName"))
                    context.RouteData.Values.Add("tenantName", TenantService.TenantName);

                base.OnActionExecuting(context);
            }
            catch (Exception e)
            {
                throw new Exception($"Current Host does not have permissions to access this resource. Host: ${tenantHost}. ${e}");
            }

        }

        
    }
}
