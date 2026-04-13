using System.Security.Cryptography.X509Certificates;

namespace ChessTournaments.Identity.Shared.Helpers;

public static class OpenIddictAzureKeyVaultProvider
{
    public static X509Certificate2 GetCertificateFromKeyVault(
        this OpenIddictServerBuilder builder,
        IConfigurationManager configurationManager,
        string secretIdentifier
    )
    {
        ArgumentNullException.ThrowIfNull(configurationManager);
        ArgumentNullException.ThrowIfNull(secretIdentifier);

        var secretString =
            configurationManager[secretIdentifier]
            ?? throw new InvalidOperationException(
                $"{secretIdentifier} certificate isn't configured."
            );
        var privateKeyBytes = Convert.FromBase64String(secretString);
        return new X509Certificate2(
            privateKeyBytes,
            (string?)null,
            X509KeyStorageFlags.MachineKeySet
        );
    }
}
