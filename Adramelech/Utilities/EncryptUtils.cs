using System.Security.Cryptography;
using System.Text;

namespace Adramelech.Utilities;

public static class EncryptUtils
{
    public static string GetHash(string password, string salt) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.Unicode.GetBytes(string.Concat(salt, password))));

    public static bool CompareHash(string password, string hash, string salt)
    {
        var bash64PasswordHash = GetHash(password, salt);

        return hash == bash64PasswordHash;
    }

    public static string GenSalt() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}