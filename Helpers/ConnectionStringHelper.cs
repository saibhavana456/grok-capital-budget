namespace IT_BUDGET_MONITORING_PORTAL.Helpers;

/// <summary>
/// Resolves connection strings the same way as Personal/SCV:
/// full AES ciphertext (U2FsdGVk…), optional ENC: prefix, or plaintext.
/// </summary>
public static class ConnectionStringHelper
{
    public static string Resolve(string? configured)
    {
        if (string.IsNullOrWhiteSpace(configured))
            return string.Empty;

        var raw = configured.Trim().Replace(" ", "+");

        if (raw.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
            return EncryptoData.DecryptAes(raw["ENC:".Length..]);

        // SCV stores the entire Organisations string as CryptoJS AES ciphertext
        if (raw.StartsWith("U2FsdGVk", StringComparison.Ordinal))
            return EncryptoData.DecryptAes(raw);

        return configured.Trim();
    }
}
