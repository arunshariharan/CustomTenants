using CustomTenants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Datastores
{
    public class UserDatastore
    {
        public static UserDatastore Current = new UserDatastore();

        public List<User> Users;

        public UserDatastore()
        {
            Users = new List<User>()
            {
                new User()
                {
                    Id = 1,
                    FullName = "A_User",
                    EmailAddress = "user.a@test.com",
                    IsAdmin = true,
                    Password = "12345678",
                    SignedUpTenantId = 1,
                    ActiveTenantIds = { 1, 2, 3 },
                    AdminForTenants = { 1, 2, 3 }
                },
                new User()
                {
                    Id = 2,
                    FullName = "B_User",
                    EmailAddress = "user.b@test.com",
                    IsAdmin = true,
                    Password = "12345678",
                    SignedUpTenantId = 2,
                    ActiveTenantIds = { 1, 2, 3 },
                    AdminForTenants = { 2 }
                },
                new User()
                {
                    Id = 3,
                    FullName = "C_User",
                    EmailAddress = "user.c@test.com",
                    IsAdmin = false,
                    Password = "12345678",
                    SignedUpTenantId = 3,
                    ActiveTenantIds = { 1, 2, 3 }
                },
                new User()
                {
                    Id = 4,
                    FullName = "D_User",
                    EmailAddress = "user.d@test.com",
                    IsAdmin = false,
                    Password = "12345678",
                    SignedUpTenantId = 1,
                    ActiveTenantIds = { 2 }
                }
            };
        }
    }
}
