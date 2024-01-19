namespace DataAccessLibraryCraftVerify
{
    public interface IWriteOnlyDAO
    {
        public int InsertAttribute(string connString, string sqlcommand);
    }
}
