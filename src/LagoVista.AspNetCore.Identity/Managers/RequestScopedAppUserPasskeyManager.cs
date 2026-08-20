using Fido2NetLib;
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.AspNetCore.Identity.Managers
{
    /// <summary>
    /// Production passkey manager that keeps the existing AppUserPasskeyManager behavior while
    /// binding both its application URL view and Fido2 verifier to the current public request origin.
    /// </summary>
    public sealed class RequestScopedAppUserPasskeyManager : AppUserPasskeyManager
    {
        public RequestScopedAppUserPasskeyManager(
            IAppUserRepo appUserRepo,
            IUserRegistrationManager userRegistrationManager,
            IAuthenticationLogManager authLogMgr,
            IAppUserPasskeyCredentialRepo credentialRepo,
            IPasskeyChallengeStore challengeStore,
            IAppConfig appConfig,
            IAdminLogger logger,
            Fido2Configuration baseFido2Configuration,
            IPasskeyRelyingPartyContext relyingPartyContext,
            IServiceProvider serviceProvider)
            : base(
                  appUserRepo,
                  userRegistrationManager,
                  authLogMgr,
                  credentialRepo,
                  challengeStore,
                  new RequestScopedPasskeyAppConfig(appConfig, relyingPartyContext),
                  logger,
                  CreateFido2(baseFido2Configuration, relyingPartyContext, serviceProvider))
        {
        }

        private static IFido2 CreateFido2(
            Fido2Configuration baseConfiguration,
            IPasskeyRelyingPartyContext relyingPartyContext,
            IServiceProvider serviceProvider)
        {
            if (baseConfiguration == null) throw new ArgumentNullException(nameof(baseConfiguration));
            if (relyingPartyContext == null) throw new ArgumentNullException(nameof(relyingPartyContext));
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            var relyingParty = relyingPartyContext.Current;
            var configuration = new Fido2Configuration
            {
                Timeout = baseConfiguration.Timeout,
                TimestampDriftTolerance = baseConfiguration.TimestampDriftTolerance,
                ChallengeSize = baseConfiguration.ChallengeSize,
                RPID = relyingParty.RpId,
                RPName = String.IsNullOrWhiteSpace(baseConfiguration.RPName) ? "NuvOS" : baseConfiguration.RPName,
                ServerIcon = baseConfiguration.ServerIcon,
                Origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { relyingParty.Origin },
                MDSCacheDirPath = baseConfiguration.MDSCacheDirPath,
                UndesiredAuthenticatorMetadataStatuses = baseConfiguration.UndesiredAuthenticatorMetadataStatuses?.ToArray(),
                BackupEligibleCredentialPolicy = baseConfiguration.BackupEligibleCredentialPolicy,
                BackedUpCredentialPolicy = baseConfiguration.BackedUpCredentialPolicy
            };

            return new Fido2(configuration, serviceProvider.GetService<IMetadataService>());
        }

        private sealed class RequestScopedPasskeyAppConfig : IAppConfig
        {
            private readonly IAppConfig _inner;
            private readonly IPasskeyRelyingPartyContext _relyingPartyContext;

            public RequestScopedPasskeyAppConfig(IAppConfig inner, IPasskeyRelyingPartyContext relyingPartyContext)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _relyingPartyContext = relyingPartyContext ?? throw new ArgumentNullException(nameof(relyingPartyContext));
            }

            public PlatformTypes PlatformType => _inner.PlatformType;
            public Environments Environment => _inner.Environment;
            public AuthTypes AuthType => _inner.AuthType;
            public EntityHeader SystemOwnerOrg => _inner.SystemOwnerOrg;
            public string WebAddress => _relyingPartyContext.Current.Origin;
            public string CompanyName => _inner.CompanyName;
            public string CompanySiteLink => _inner.CompanySiteLink;
            public string AppName => _inner.AppName;
            public string AppId => _inner.AppId;
            public string APIToken => _inner.APIToken;
            public string AppDescription => _inner.AppDescription;
            public string TermsAndConditionsLink => _inner.TermsAndConditionsLink;
            public string PrivacyStatementLink => _inner.PrivacyStatementLink;
            public string ClientType => _inner.ClientType;
            public string AppLogo => _inner.AppLogo;
            public string CompanyLogo => _inner.CompanyLogo;
            public string InstanceId { get => _inner.InstanceId; set => _inner.InstanceId = value; }
            public string InstanceAuthKey { get => _inner.InstanceAuthKey; set => _inner.InstanceAuthKey = value; }
            public string DeviceId { get => _inner.DeviceId; set => _inner.DeviceId = value; }
            public string DeviceRepoId { get => _inner.DeviceRepoId; set => _inner.DeviceRepoId = value; }
            public string DefaultDeviceLabel => _inner.DefaultDeviceLabel;
            public string DefaultDeviceLabelPlural => _inner.DefaultDeviceLabelPlural;
            public bool EmitTestingCode => _inner.EmitTestingCode;
            public VersionInfo Version => _inner.Version;
            public string AnalyticsKey { get => _inner.AnalyticsKey; set => _inner.AnalyticsKey = value; }
        }
    }
}
