using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MultiTenant.Datastores;
using MultiTenant.Models;
using MultiTenant.Services;

namespace MultiTenant.Repositories
{
    public class UserRepository : IUserRepository
    {
        public User GetUser(int userId)
        {
            var users = GetUsers();
            if (users == null) return null;

            return users.FirstOrDefault(u => u.Id == userId);
        }

        public User GetUser(string emailAddress)
        {
            var users = GetUsers();
            if (users == null) return null;

            return users.FirstOrDefault(u => u.EmailAddress == emailAddress);
        }

        public IEnumerable<User> GetUsers()
        {
            int _tenantId = TenantService.TenantId;
            return UserDatastore.Current.Users.Where(u => u.ActiveTenantIds.Contains(_tenantId));
        }

        public void MakeAdmin(User user)
        {
            if (!user.AdminForTenants.Contains(TenantService.TenantId))
            {
                user.AdminForTenants.Add(TenantService.TenantId);
            }
        }

        public void RemoveAdmin(User user)
        {
            if (user.AdminForTenants.Contains(TenantService.TenantId))
            {
                user.AdminForTenants.Remove(TenantService.TenantId);
            }
        }

        public User CreateUser(User user)
        {
            int _tenantId = TenantService.TenantId;
            List<int> tenantIds = TenantService.AllTenantIds;

            User lastUser = UserDatastore.Current.Users.LastOrDefault();

            user.Id = lastUser.Id + 1;
            user.SignedUpTenantId = _tenantId;
            user.ActiveTenantIds = tenantIds;

            UserDatastore.Current.Users.Add(user);
            
            return user;
        }

        public bool ValidatePassword(UserCredentials userCred, User user)
        {
            return (user.EmailAddress == userCred.EmailAddress && user.Password == userCred.Password);
        }

        public bool ValidatePassword(string passwordToValidate, User user)
        {
            return (user.Password == passwordToValidate);
        }

        public void DeactivateUser(User user)
        {
            if(user.ActiveTenantIds.Contains(TenantService.TenantId))
            {
                user.ActiveTenantIds.Remove(TenantService.TenantId);
                user.DeactivatedOnTenants.Add(TenantService.TenantId);
            }
        }

        public void ActivateUser(User user)
        {
            if (!user.ActiveTenantIds.Contains(TenantService.TenantId))
            {
                user.ActiveTenantIds.Add(TenantService.TenantId);
                user.DeactivatedOnTenants.Remove(TenantService.TenantId);
            }
        }

        public User GetDeactivatedUser(string emailAddress)
        {
            var deactivatedUsersList = GetDeactivatedUsers();
            return deactivatedUsersList.FirstOrDefault(user => user.EmailAddress == emailAddress);
        }

        public IEnumerable<User> GetDeactivatedUsers()
        {
            int _tenantId = TenantService.TenantId;
            return UserDatastore.Current.Users.Where(u => u.DeactivatedOnTenants.Contains(_tenantId));
        }

        public User GetUser(IEnumerable<Claim> claim)
        {
            var claimEmailAddress = claim.FirstOrDefault(c => c.Type == "Email").Value.ToString();
            var user = GetUser(claimEmailAddress);
            return user;
        }

        public void UpdatePassword(string passwordToUpdated, User user)
        {
            user.Password = passwordToUpdated;
        }

        public bool IsAdmin(User user)
        {
            return user.AdminForTenants.Contains(TenantService.TenantId);
        }
    }
}
