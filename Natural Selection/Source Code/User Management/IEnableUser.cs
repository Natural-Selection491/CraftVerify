using NaturalSelection.DataAccess;

namespace NaturalSelection.UserManagement.EnableAccount
{
    public interface IEnableUser
    {
        public bool EnableUSer(string userHash);
    }
}
