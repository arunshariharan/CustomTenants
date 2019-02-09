using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public int SignedUpTenantId { get; set; }

        public List<int> AdminForTenants = new List<int>();
        
        public List<int> ActiveTenantIds = new List<int>();
        public List<int> DeactivatedOnTenants = new List<int>();
    }
}
