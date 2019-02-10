using MultiTenant.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenant.Services
{
    public class TenantService
    {
        public static int TenantId { get; set; }
        public static string TenantName { get; set; }
        public static string TenantHost { get; set; }

        public static List<int> AllTenantIds = new List<int>();
        public static List<string> AllTenantNames = new List<string>();
        public static List<string> AllTenantHosts = new List<string>();

        public static int GetCurrentTenantId(string host)
        {
            JObject knownTenants = GetTenants();
            var currentTenant = GetCurrentTenant(knownTenants, host);

            if(currentTenant == null)
                throw new Exception($"Current Host: ${host} could not be found");
            
            TenantId = Convert.ToInt32(currentTenant["Id"]);

            return TenantId;
        }

        public static void SetTenantDetails(string host)
        {
            JObject knownTenants = GetTenants();
            var currentTenant = GetCurrentTenant(knownTenants, host);

            if (currentTenant == null)
                throw new Exception($"Host with host name ${host} could not be found");

            TenantName = currentTenant["Name"].ToString();
            TenantHost = host;
            if (AllTenantIds.Count == 0)
            {
                RetrieveAndSetAllTenantIds(knownTenants);
                RetrieveAndSetAllTenantNames(knownTenants);
                RetrieveAndSetAllTenantHosts(knownTenants);
            }
        }

        private static void RetrieveAndSetAllTenantIds(JObject knownTenants)
        {
            foreach (var tenant in knownTenants["Tenants"])
            {
                AllTenantIds.Add(Convert.ToInt32(tenant["Id"]));
            }
        }

        private static void RetrieveAndSetAllTenantNames(JObject knownTenants)
        {
            foreach (var tenant in knownTenants["Tenants"])
            {
                AllTenantNames.Add(tenant["Name"].ToString());
            }
        }

        private static void RetrieveAndSetAllTenantHosts(JObject knownTenants)
        {
            foreach (var tenant in knownTenants["Tenants"])
            {
                AllTenantHosts.Add(tenant["HostPort"].ToString());
            }
        }

        public static JToken GetCurrentTenant(JObject knownTenants, string host)
        {
            foreach (var tenant  in knownTenants["Tenants"])
            {
                if (host.Contains(tenant["HostPort"].ToString()))
                    return tenant;
            }

            return null;
        }

        public static JObject GetTenants()
        {
            using (StreamReader reader = new StreamReader("Tenants.json"))
            {
                var tenantsList = reader.ReadToEnd();
                var tenantsObject = JObject.Parse(tenantsList);
                return tenantsObject;
            }
        }
    }
}
