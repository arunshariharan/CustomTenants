using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Formatters
{
    public class ErrorFormatter
    {
        public static List<string> FormatValidationErrors(string errors)
        {
            List<string> errorList = new List<string>();
            foreach (var error in errors.Split("\n"))
            {
                errorList.Add(error);
            }
            return errorList;
        }
    }
}
