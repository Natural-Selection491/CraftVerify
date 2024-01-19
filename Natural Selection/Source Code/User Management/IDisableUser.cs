using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalSelection.UserManagement.DisableAccount
{
    public interface IDisableUser
    {
        public bool DisableAccount(string userHash);
    }
}
