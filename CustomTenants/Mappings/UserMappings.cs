using AutoMapper;
using CustomTenants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Mappings
{
    public class UserMappings : IUserMappings
    {
        public UserWithoutSensitiveDataDto StripSensitiveDataSingleUser(User user)
        {
            return Mapper.Map<UserWithoutSensitiveDataDto>(user);
        }

        public List<UserWithoutSensitiveDataDto> StripSensitiveDataMultipleUsers(IEnumerable<User> users)
        {
            var mappedUsers = new List<UserWithoutSensitiveDataDto>();
            foreach (var user in users)
            {
                mappedUsers.Add(Mapper.Map<UserWithoutSensitiveDataDto>(user));
            }

            return mappedUsers;
        }
    }
}
