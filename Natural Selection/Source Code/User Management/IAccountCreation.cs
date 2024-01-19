using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftVerify.Natural.UserManagement
{
    public interface IAccountCreation
    {
        int userID { get; set; }
        string email { get; set; }
        DateTime DOB { get; set; }
        string userRole { get; set; }
        bool userStatus { get; set; }

        void SetAdditionalAttribute(string key, object value);
        object GetAdditionalAttribute(string key);

    }
}
