using LagoVista.UserAdmin.Models.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.AuthorizationServer.Persistence.UserAdmin
{
    /// <summary>
    /// Validates OAuth client secrets stored encrypted by LagoVista ISecureStorage.
    /// The application store returns the decrypted secret at validation time, so the
    /// default OpenIddict PBKDF2 comparison is replaced with a fixed-time comparison.
    /// </summary>
    public sealed class LagoVistaOpenIddictApplicationManager : OpenIddictApplicationManager<OAuthClientApplication>
    {
        public LagoVistaOpenIddictApplicationManager(
            IOpenIddictApplicationCache<OAuthClientApplication> cache,
            ILogger<OpenIddictApplicationManager<OAuthClientApplication>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options,
            IOpenIddictApplicationStore<OAuthClientApplication> store)
            : base(cache, logger, options, store)
        {
        }

        protected override ValueTask<bool> ValidateClientSecretAsync(
            string secret,
            string comparand,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(secret);
            ArgumentException.ThrowIfNullOrEmpty(comparand);
            cancellationToken.ThrowIfCancellationRequested();

            var secretHash = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            var comparandHash = SHA256.HashData(Encoding.UTF8.GetBytes(comparand));

            return new ValueTask<bool>(CryptographicOperations.FixedTimeEquals(secretHash, comparandHash));
        }
    }
}
