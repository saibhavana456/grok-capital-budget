using System.Security.Cryptography;
using System.Text;

namespace IT_BUDGET_MONITORING_PORTAL.Helpers;

/// <summary>
/// AES helpers matching Personal/SCV EncryptoData (production login pattern).
/// DecryptAes  → CryptoJS AES passphrase "01234567890" (Salted__)
/// EncryptString / DecryptString → fixed 32-char key, zero IV (response / USER_TOKEN.USERID)
/// </summary>
public static class EncryptoData
{
    private const string AesPassPhrase = "01234567890";
    private const string FixedKey = "01234567890123456789012345678901";

    private static void DeriveKeyAndIv(byte[] phrase, byte[]? salt, int iterations, out byte[] key, out byte[] iv)
    {
        var hashList = new List<byte>();
        var preHashLength = phrase.Length + (salt?.Length ?? 0);
        var preHash = new byte[preHashLength];
        Buffer.BlockCopy(phrase, 0, preHash, 0, phrase.Length);
        if (salt != null)
            Buffer.BlockCopy(salt, 0, preHash, phrase.Length, salt.Length);

        using var hash = MD5.Create();
        var currentHash = hash.ComputeHash(preHash);
        for (var i = 1; i < iterations; i++)
            currentHash = hash.ComputeHash(currentHash);
        hashList.AddRange(currentHash);

        while (hashList.Count < 48)
        {
            preHashLength = currentHash.Length + phrase.Length + (salt?.Length ?? 0);
            preHash = new byte[preHashLength];
            Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
            Buffer.BlockCopy(phrase, 0, preHash, currentHash.Length, phrase.Length);
            if (salt != null)
                Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + phrase.Length, salt.Length);

            currentHash = hash.ComputeHash(preHash);
            for (var i = 1; i < iterations; i++)
                currentHash = hash.ComputeHash(currentHash);
            hashList.AddRange(currentHash);
        }

        key = new byte[32];
        iv = new byte[16];
        hashList.CopyTo(0, key, 0, 32);
        hashList.CopyTo(32, iv, 0, 16);
    }

    /// <summary>Decrypt CryptoJS AES (used for encrypted appsettings / AD-style payloads).</summary>
    public static string DecryptAes(string encrypted)
    {
        var encryptedString = encrypted.Replace(" ", "+");
        var base64Bytes = Convert.FromBase64String(encryptedString);
        var saltBytes = base64Bytes[8..16];
        var cipherTextBytes = base64Bytes[16..];
        var phraseBytes = Encoding.UTF8.GetBytes(AesPassPhrase);
        DeriveKeyAndIv(phraseBytes, saltBytes, 1, out var keyBytes, out var ivBytes);

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;
        aes.KeySize = 256;
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.CBC;
        using var msDecrypt = new MemoryStream(cipherTextBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(keyBytes, ivBytes), CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    public static string EncryptString(string plainText)
    {
        byte[] iv = new byte[16];
        byte[] array;
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(FixedKey);
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }
            array = memoryStream.ToArray();
        }
        return Convert.ToBase64String(array);
    }

    public static string DecryptString(string cipherText)
    {
        var buffer = Convert.FromBase64String(cipherText.Replace(" ", "+"));
        byte[] iv = new byte[16];
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(FixedKey);
        aes.IV = iv;
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.CBC;
        using var memoryStream = new MemoryStream(buffer);
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream);
        return reader.ReadToEnd();
    }

    public static string Sha256Base64(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
