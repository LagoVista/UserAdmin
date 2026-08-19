using LagoVista.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace LagoVista.AspNetCore.AuthorizationServer.Security
{
    /// <summary>
    /// Loads the shared OpenIddict signing/encryption certificate from ISecureStorage.
    /// If the versioned secret does not exist, the first pod creates it and subsequent
    /// pods read the same certificate. This configuration runs once when OpenIddict
    /// materializes its server options.
    /// </summary>
    public sealed class OpenIddictSecureStorageCredentialConfigurator : IConfigureOptions<OpenIddictServerOptions>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AuthorizationServerOptions _settings;

        public OpenIddictSecureStorageCredentialConfigurator(
            IServiceScopeFactory scopeFactory,
            AuthorizationServerOptions settings)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Configure(OpenIddictServerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // Local development keeps using OpenIddict's development certificates.
            if (_settings.UseDevelopmentCertificates)
                return;

            if (String.IsNullOrWhiteSpace(_settings.SharedCertificateSecretId))
                throw new InvalidOperationException("A shared OpenIddict certificate secret id is required when development certificates are disabled.");

            var certificate = LoadOrCreateCertificate();
            var key = new X509SecurityKey(certificate);

            options.SigningCredentials.Add(new SigningCredentials(
                key,
                SecurityAlgorithms.RsaSha256));

            options.EncryptionCredentials.Add(new EncryptingCredentials(
                key,
                SecurityAlgorithms.RsaOAEP,
                SecurityAlgorithms.Aes256CbcHmacSha512));
        }

        private X509Certificate2 LoadOrCreateCertificate()
        {
            // IConfigureOptions<T> is registered as a singleton, while secure storage and
            // some of its dependencies may be scoped. Resolve those services inside a
            // short-lived startup scope instead of capturing them in the singleton.
            using var scope = _scopeFactory.CreateScope();
            var secureStorage = scope.ServiceProvider.GetRequiredService<ISecureStorage>();
            var appConfig = scope.ServiceProvider.GetRequiredService<IAppConfig>();
            var systemUsers = scope.ServiceProvider.GetRequiredService<ISystemUsers>();

            var org = appConfig.SystemOwnerOrg ?? systemUsers.SystemOrg;
            var user = systemUsers.HostUser;

            if (org == null)
                throw new InvalidOperationException("The system owner organization is required to load OpenIddict key material.");

            if (user == null)
                throw new InvalidOperationException("The host system user is required to load OpenIddict key material.");

            var get = secureStorage
                .GetSecretAsync(org, _settings.SharedCertificateSecretId, user)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            if (!get.Successful || String.IsNullOrWhiteSpace(get.Result))
            {
                var generated = CreateCertificate();
                var payload = Convert.ToBase64String(generated.Export(X509ContentType.Pkcs12));

                var add = secureStorage
                    .AddSecretAsync(org, _settings.SharedCertificateSecretId, payload)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                // Another pod may have won the create race. Always re-read the canonical
                // secret instead of assuming the locally generated certificate is authoritative.
                get = secureStorage
                    .GetSecretAsync(org, _settings.SharedCertificateSecretId, user)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                if (!get.Successful || String.IsNullOrWhiteSpace(get.Result))
                {
                    var addError = add?.ErrorMessage;
                    throw new InvalidOperationException(
                        $"Could not create or load shared OpenIddict certificate '{_settings.SharedCertificateSecretId}'. {addError}");
                }
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(get.Result);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    $"Shared OpenIddict certificate '{_settings.SharedCertificateSecretId}' is not valid base64 PKCS#12 data.", ex);
            }

            return new X509Certificate2(
                bytes,
                (string)null,
                X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
        }

        private static X509Certificate2 CreateCertificate()
        {
            using var rsa = RSA.Create(3072);
            var request = new CertificateRequest(
                "CN=NuvIoT OpenIddict Server",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: true));

            var now = DateTimeOffset.UtcNow;
            return request.CreateSelfSigned(now.AddMinutes(-5), now.AddYears(5));
        }
    }
}
