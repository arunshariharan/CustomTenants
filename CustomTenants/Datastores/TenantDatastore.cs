using CustomTenants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Datastores
{
    public class TenantDatastore
    {
        public static TenantDatastore Current = new TenantDatastore();
        public List<Tenant> Tenants;

        public TenantDatastore()
        {
            Tenants = new List<Tenant>()
            {
                new Tenant()
                {
                    Id = 1,
                    Name = "Tenant A",
                    Host = "localhost:1111",
                    RegisteredUsers = UserDatastore.Current.Users.FindAll(u => u.ActiveTenantIds.Contains(1))
                },
                new Tenant()
                {
                    Id = 2,
                    Name = "Tenant B",
                    Host = "localhost:2222",
                    RegisteredUsers = UserDatastore.Current.Users.FindAll(u => u.ActiveTenantIds.Contains(2))
                },
                new Tenant()
                {
                    Id = 3,
                    Name = "Tenant A",
                    Host = "localhost:3333",
                    RegisteredUsers = UserDatastore.Current.Users.FindAll(u => u.ActiveTenantIds.Contains(3))
                }
            };
        }
    }
}
