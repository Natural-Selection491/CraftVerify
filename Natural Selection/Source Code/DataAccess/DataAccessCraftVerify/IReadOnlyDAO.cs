namespace DataAccessLibraryCraftVerify
{
    public interface IReadOnlyDAO
    {
        public ICollection<object>? GetAttribute(string sqlcommand);
    }
}
