using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Services
{
    public class TenantService
    {
        public static int TenantId { get; private set; }
        public static string TenantName { get; private set; }
        public static string TenantHost { get; private set; }


        public static int GetCurrentTenantId(string host)
        {
            string tenantIdConfiguration = "Tenants:" + host + ":Id";
            var tenantId = Startup.Configuration[tenantIdConfiguration];
            if (tenantId == null) throw new Exception($"Host with host name ${host} could not be found");

            TenantId = Convert.ToInt32(tenantId);

            return TenantId;
        }

        public static void SetTenantNameAndHost(string host)
        {
            string tenantNameConfiguration = "Tenants:" + host + ":Name";
            TenantName = Startup.Configuration[tenantNameConfiguration];
            TenantHost = host;
        }
    }
}
