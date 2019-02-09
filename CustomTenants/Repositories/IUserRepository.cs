using CustomTenants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User GetUser(int userId);
        User GetUser(string emailAddress);
        User GetDeactivatedUser(string emailAddress);
        void MakeAdmin(User user);
        void RemoveAdmin(User user);

        User CreateUser(User user);

        bool ValidatePassword(UserCredentials userCred, User user);

        void DeactivateUser(User user);
        void ActivateUser(User user);


    }
}
