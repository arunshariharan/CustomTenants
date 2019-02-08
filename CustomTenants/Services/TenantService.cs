using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Services
{
    public class TenantService
    {
        public static int GetTenantId(string host)
        {
            string tenantIdConfiguration = "Tenants:" + host + ":Id";
            var tenantId = Startup.Configuration[tenantIdConfiguration];
            if (tenantId == null) return 0;

            return Convert.ToInt32(tenantId);
        }
    }
}
