using System.Security.Cryptography;
using System.Text;

namespace Adramelech.Utilities;

/// <summary>
/// Utility class for encrypting and decrypting data
/// </summary>
public static class EncryptUtils
{
    /// <summary>
    /// Encrypts the specified text using the specified key.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="key">The key.</param>
    /// <returns>The encrypted text.</returns>
    public static async Task<string> Encrypt(string text, string key)
    {
        var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.IV = new byte[16]; // Initialization vector
        aesAlg.Padding = PaddingMode.PKCS7;

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using var output = new MemoryStream();
        await using var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write);

        await cryptoStream.WriteAsync(Encoding.UTF8.GetBytes(text));
        await cryptoStream.FlushFinalBlockAsync();

        return Convert.ToBase64String(output.ToArray());
    }

    /// <summary>
    /// Decrypts the specified cipher text using the specified key.
    /// </summary>
    /// <param name="cipherText">The cipher text.</param>
    /// <param name="key">The key.</param>
    /// <returns>The decrypted text.</returns>
    public static async Task<string> Decrypt(string cipherText, string key)
    {
        var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.IV = new byte[16]; // Initialization vector
        aesAlg.Padding = PaddingMode.PKCS7;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
        await using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return await srDecrypt.ReadToEndAsync();
    }

    /// <summary>
    /// Derives a key from the specified password.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>The derived key.</returns>
    public static string DeriveKey(string password)
    {
        var emptySalt = Array.Empty<byte>();
        const int iterations = 1000;
        const int derivedKeyLength = 16; // 16 == 128 bits
        var hashMethod = HashAlgorithmName.SHA384;

        var key = Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
            emptySalt, iterations, hashMethod, derivedKeyLength);

        return Convert.ToBase64String(key);
    }
}