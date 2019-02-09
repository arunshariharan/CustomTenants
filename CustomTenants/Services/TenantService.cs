﻿using CustomTenants.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Services
{
    public class TenantService
    {
        public static int TenantId { get; private set; }
        public static string TenantName { get; private set; }
        public static string TenantHost { get; private set; }

        public static List<int> AllTenantIds = new List<int>();

        private readonly static string tenantsFilepath = Startup.Configuration["TenantsFilepath"];


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
            if(AllTenantIds.Count == 0) RetrieveAndSetAllTenantIds(knownTenants);            
        }

        private static void RetrieveAndSetAllTenantIds(JObject knownTenants)
        {
            foreach (var tenant in knownTenants["Tenants"])
            {
                AllTenantIds.Add(Convert.ToInt32(tenant["Id"]));
            }
        }

        public static JToken GetCurrentTenant(JObject knownTenants, string host)
        {
            foreach (var tenant  in knownTenants["Tenants"])
            {
                if (tenant["Host"].ToString() == host)
                    return tenant;
            }

            return null;
        }

        public static JObject GetTenants()
        {
            using (StreamReader reader = new StreamReader(tenantsFilepath))
            {
                var tenantsList = reader.ReadToEnd();
                var tenantsObject = JObject.Parse(tenantsList);
                return tenantsObject;
            }
        }
    }
}
