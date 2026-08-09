using Azure;
using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentEntityRepo : TableStorageBase<ProvisionalEnvironmentEntity>, IProvisionalEnvironmentEntityRepo
    {
        public ProvisionalEnvironmentEntityRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "provisionalenvironment";
        }

        public Task InsertAsync(ProvisionalEnvironment environment)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            return base.InsertAsync(ToEntity(environment));
        }

        public async Task<ProvisionalEnvironment> GetByIdAsync(string id)
        {
            var entity = await GetAsync(ProvisionalEnvironmentEntity.CreatePartitionKey(id), ProvisionalEnvironmentEntity.CreateRowKey(id), false);
            return entity == null ? null : ToModel(entity);
        }

        public Task UpdateAsync(ProvisionalEnvironment environment)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            return base.UpdateAsync(ToEntity(environment));
        }

        public Task DeleteAsync(string id)
        {
            return RemoveAsync(ProvisionalEnvironmentEntity.CreatePartitionKey(id), ProvisionalEnvironmentEntity.CreateRowKey(id));
        }

        private static ProvisionalEnvironmentEntity ToEntity(ProvisionalEnvironment environment)
        {
            var entity = new ProvisionalEnvironmentEntity
            {
                PartitionKey = ProvisionalEnvironmentEntity.CreatePartitionKey(environment.Id),
                RowKey = ProvisionalEnvironmentEntity.CreateRowKey(environment.Id),
                Id = environment.Id,
                State = environment.State.ToString(),
                CreationRequestId = environment.CreationRequestId,
                OriginActorId = environment.OriginActorId,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                RecoveryTokenHash = environment.RecoveryTokenHash,
                InstallationIdHash = environment.InstallationIdHash,
                BootstrapContext = environment.BootstrapContext,
                TermsAndConditionsAccepted = environment.TermsAndConditionsAccepted,
                TermsAndConditionsVersion = environment.TermsAndConditionsVersion,
                TermsAndConditionsAcceptedIPAddress = environment.TermsAndConditionsAcceptedIPAddress,
                TermsAndConditionsAcceptedUtc = environment.TermsAndConditionsAcceptedUtc?.ToString("O"),
                CreatedUtc = environment.CreatedUtc.ToString("O"),
                ActivatedUtc = environment.ActivatedUtc?.ToString("O"),
                LastActivityUtc = environment.LastActivityUtc.ToString("O"),
                ExpiresUtc = environment.ExpiresUtc.ToString("O"),
                ClaimedUtc = environment.ClaimedUtc?.ToString("O"),
                ExpiredUtc = environment.ExpiredUtc?.ToString("O"),
                PurgeAfterUtc = environment.PurgeAfterUtc?.ToString("O"),
                StateChangedUtc = environment.StateChangedUtc.ToString("O"),
                ConversionJourneyId = environment.ConversionJourneyId,
                AcquisitionSourceKey = environment.AcquisitionSourceKey,
                CampaignKey = environment.CampaignKey,
                EntryPointType = environment.EntryPointType,
                EntryPointKey = environment.EntryPointKey,
                ExperimentKey = environment.ExperimentKey,
                ExperimentVariantKey = environment.ExperimentVariantKey,
                AgentKey = environment.AgentKey,
                AgentVersion = environment.AgentVersion,
                PromptVersion = environment.PromptVersion
            };

            if (!String.IsNullOrEmpty(environment.ETag)) entity.ETag = environment.ETag;
            return entity;
        }

        private static ProvisionalEnvironment ToModel(ProvisionalEnvironmentEntity entity)
        {
            return new ProvisionalEnvironment
            {
                Id = entity.Id,
                State = Enum.Parse<ProvisionalEnvironmentState>(entity.State),
                CreationRequestId = entity.CreationRequestId,
                OriginActorId = entity.OriginActorId,
                AppUserId = entity.AppUserId,
                OrganizationId = entity.OrganizationId,
                SubscriptionId = entity.SubscriptionId,
                RecoveryTokenHash = entity.RecoveryTokenHash,
                InstallationIdHash = entity.InstallationIdHash,
                BootstrapContext = entity.BootstrapContext,
                TermsAndConditionsAccepted = entity.TermsAndConditionsAccepted,
                TermsAndConditionsVersion = entity.TermsAndConditionsVersion,
                TermsAndConditionsAcceptedIPAddress = entity.TermsAndConditionsAcceptedIPAddress,
                TermsAndConditionsAcceptedUtc = ParseNullableUtc(entity.TermsAndConditionsAcceptedUtc),
                CreatedUtc = ParseUtc(entity.CreatedUtc),
                ActivatedUtc = ParseNullableUtc(entity.ActivatedUtc),
                LastActivityUtc = ParseUtc(entity.LastActivityUtc),
                ExpiresUtc = ParseUtc(entity.ExpiresUtc),
                ClaimedUtc = ParseNullableUtc(entity.ClaimedUtc),
                ExpiredUtc = ParseNullableUtc(entity.ExpiredUtc),
                PurgeAfterUtc = ParseNullableUtc(entity.PurgeAfterUtc),
                StateChangedUtc = ParseUtc(entity.StateChangedUtc),
                ConversionJourneyId = entity.ConversionJourneyId,
                AcquisitionSourceKey = entity.AcquisitionSourceKey,
                CampaignKey = entity.CampaignKey,
                EntryPointType = entity.EntryPointType,
                EntryPointKey = entity.EntryPointKey,
                ExperimentKey = entity.ExperimentKey,
                ExperimentVariantKey = entity.ExperimentVariantKey,
                AgentKey = entity.AgentKey,
                AgentVersion = entity.AgentVersion,
                PromptVersion = entity.PromptVersion,
                ETag = entity.ETag
            };
        }

        private static DateTime ParseUtc(string value)
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        private static DateTime? ParseNullableUtc(string value)
        {
            return String.IsNullOrEmpty(value) ? (DateTime?)null : ParseUtc(value);
        }
    }
}
