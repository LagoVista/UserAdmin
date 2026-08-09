using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal class AnonymousVisitorEntityRepo : TableStorageBase<AnonymousVisitorEntity>, IAnonymousVisitorEntityRepo
    {
        public AnonymousVisitorEntityRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "anonymousvisitor";
        }

        public Task InsertAsync(AnonymousVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return base.InsertAsync(ToEntity(visitor));
        }

        public async Task<AnonymousVisitor> GetByActorIdAsync(string actorId)
        {
            var entity = await GetAsync(AnonymousVisitorEntity.CreatePartitionKey(actorId), AnonymousVisitorEntity.CreateRowKey(actorId), false);
            return entity == null ? null : ToModel(entity);
        }

        public Task UpdateAsync(AnonymousVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return base.UpdateAsync(ToEntity(visitor));
        }

        public Task DeleteAsync(string actorId)
        {
            return RemoveAsync(AnonymousVisitorEntity.CreatePartitionKey(actorId), AnonymousVisitorEntity.CreateRowKey(actorId));
        }

        private static AnonymousVisitorEntity ToEntity(AnonymousVisitor visitor)
        {
            var entity = new AnonymousVisitorEntity
            {
                PartitionKey = AnonymousVisitorEntity.CreatePartitionKey(visitor.ActorId),
                RowKey = AnonymousVisitorEntity.CreateRowKey(visitor.ActorId),
                ActorId = visitor.ActorId,
                State = visitor.State.ToString(),
                ContinuityTokenHash = visitor.ContinuityTokenHash,
                InstallationIdHash = visitor.InstallationIdHash,
                BootstrapContext = visitor.BootstrapContext,
                CreatedUtc = visitor.CreatedUtc.ToUniversalTime().ToString("O"),
                LastActivityUtc = visitor.LastActivityUtc.ToUniversalTime().ToString("O"),
                ExpiresUtc = visitor.ExpiresUtc.ToUniversalTime().ToString("O"),
                StateChangedUtc = visitor.StateChangedUtc.ToUniversalTime().ToString("O"),
                ProvisionalEnvironmentId = visitor.ProvisionalEnvironmentId,
                PromotedUtc = visitor.PromotedUtc?.ToUniversalTime().ToString("O"),
                ExpiredUtc = visitor.ExpiredUtc?.ToUniversalTime().ToString("O"),
                ConversionJourneyId = visitor.ConversionJourneyId,
                AcquisitionSourceKey = visitor.AcquisitionSourceKey,
                CampaignKey = visitor.CampaignKey,
                EntryPointType = visitor.EntryPointType,
                EntryPointKey = visitor.EntryPointKey,
                ExperimentKey = visitor.ExperimentKey,
                ExperimentVariantKey = visitor.ExperimentVariantKey,
                AgentKey = visitor.AgentKey,
                AgentVersion = visitor.AgentVersion,
                PromptVersion = visitor.PromptVersion
            };

            if (!String.IsNullOrEmpty(visitor.ETag)) entity.ETag = visitor.ETag;
            return entity;
        }

        private static AnonymousVisitor ToModel(AnonymousVisitorEntity entity)
        {
            return new AnonymousVisitor
            {
                ActorId = entity.ActorId,
                State = Enum.Parse<AnonymousVisitorState>(entity.State),
                ContinuityTokenHash = entity.ContinuityTokenHash,
                InstallationIdHash = entity.InstallationIdHash,
                BootstrapContext = entity.BootstrapContext,
                CreatedUtc = ParseUtc(entity.CreatedUtc),
                LastActivityUtc = ParseUtc(entity.LastActivityUtc),
                ExpiresUtc = ParseUtc(entity.ExpiresUtc),
                StateChangedUtc = ParseUtc(entity.StateChangedUtc),
                ProvisionalEnvironmentId = entity.ProvisionalEnvironmentId,
                PromotedUtc = ParseNullableUtc(entity.PromotedUtc),
                ExpiredUtc = ParseNullableUtc(entity.ExpiredUtc),
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
