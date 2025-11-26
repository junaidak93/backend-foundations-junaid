using System.Security.Cryptography;
using System.Text;

namespace NotesApi.Helpers;

public class StringHasher(IConfiguration config) : IStringHasher
{
    private readonly byte[] _key = Encoding.UTF8.GetBytes(config[Constants.KEY_SECRET]!);
    private readonly string _salt = config[Constants.KEY_SALT]!;

    public string GetSHA256Hash(string input)
    {
        // Convert the input string to a byte array and compute the hash
        return HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(input + _salt)).ToHex();
    }

    public string GetMD5Hash(string input)
    {
        // Convert the input string to a byte array and compute the hash
        return MD5.HashData(Encoding.UTF8.GetBytes(input + _salt)).ToHex();
    }
}