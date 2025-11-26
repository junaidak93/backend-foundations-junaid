public interface IStringHasher
{
    string GetSHA256Hash(string input);
    string GetMD5Hash(string input);
}