using System.Security.Cryptography;
using System.Text;

namespace Adramelech.Utilities;

/// <summary>
/// Utility class for encrypting and decrypting data
/// </summary>
public static class EncryptUtils
{
    /// <summary>
    /// Generates a hash from a password and salt
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <param name="salt">The salt to use</param>
    /// <returns>The hashed password</returns>
    /// <remarks>Throws the same as <see cref="Encoding.GetBytes(string)"/>, <see cref="SHA256.HashData(byte[])"/> and <see cref="Convert.ToBase64String(byte[])"/>.</remarks>
    public static string GetHash(string password, string salt) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.Unicode.GetBytes(string.Concat(salt, password))));

    /// <summary>
    /// Compares a password to a hash and salt
    /// </summary>
    /// <param name="password">The password to compare</param>
    /// <param name="hash">The hash to compare</param>
    /// <param name="salt">The salt to use</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    /// <remarks>Throws the same as <see cref="GetHash(string, string)"/>.</remarks>
    public static bool CompareHash(string password, string hash, string salt)
    {
        var bash64PasswordHash = GetHash(password, salt);

        return hash == bash64PasswordHash;
    }

    /// <summary>
    /// Generates a random salt
    /// </summary>
    /// <returns>The salt</returns>
    /// <remarks>Throws the same as <see cref="Convert.ToBase64String(byte[])"/>.</remarks>
    public static string GenSalt() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}