using MultiTenant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenant.Mappings
{
    public interface IUserMappings
    {
        UserWithoutSensitiveDataDto StripSensitiveDataSingleUser(User user);
        List<UserWithoutSensitiveDataDto> StripSensitiveDataMultipleUsers(IEnumerable<User> users);
    }
}
