using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Models
{
    public class UserWithoutSensitiveDataDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }

        public List<int> AdminForTenants = new List<int>();

        public List<int> ActiveTenantIds = new List<int>();
    }
}
