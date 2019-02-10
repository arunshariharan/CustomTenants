using CustomTenants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CustomTenants.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User GetUser(int userId);
        User GetUser(string emailAddress);
        User GetUser(IEnumerable<Claim> claim);
        User GetDeactivatedUser(string emailAddress);
        void MakeAdmin(User user);
        void RemoveAdmin(User user);

        User CreateUser(User user);

        bool ValidatePassword(UserCredentials userCred, User user);
        bool ValidatePassword(string passwordToValidate, User user);

        void DeactivateUser(User user);
        void ActivateUser(User user);
        void UpdatePassword(string passwordToUpdated, User user);


    }
}
