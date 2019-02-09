using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomTenants.Datastores;
using CustomTenants.Models;
using CustomTenants.Services;

namespace CustomTenants.Repositories
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
            if (!user.IsAdmin) user.IsAdmin = false;
            user.SignedUpTenantId = _tenantId;
            user.ActiveTenantIds = tenantIds;

            UserDatastore.Current.Users.Add(user);
            
            return user;
        }

        public bool ValidatePassword(UserCredentials userCred, User user)
        {
            return (user.EmailAddress == userCred.EmailAddress && user.Password == userCred.Password);
        }
        
    }
}
