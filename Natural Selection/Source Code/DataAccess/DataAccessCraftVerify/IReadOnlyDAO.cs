namespace DataAccessLibraryCraftVerify
{
    public interface IReadOnlyDAO
    {
        public ICollection<object>? GetAttribute(string connString, string sqlcommand);
    }
}
