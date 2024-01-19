public interface IHashService
{
    Task<string> HashValueAsync(string value, string userIdentity);
}
