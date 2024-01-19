using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalSelection.UserManagement.AccountRecovery
{
    public interface IRecoverAccount
    {
        public bool RecoverUserAccount();

        public bool LogRecoveryRequest(string userHash);
    }
}
