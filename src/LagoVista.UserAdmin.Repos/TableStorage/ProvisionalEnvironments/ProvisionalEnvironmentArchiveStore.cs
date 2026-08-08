using Azure.Storage.Blobs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentArchiveStore : IProvisionalEnvironmentArchiveStore
    {
        private const string ContainerName = "provisional-environment-archives";
        private const string ManifestFileName = "manifest.json";

        private static readonly string[] BillingEventColumns =
        {
            "Id", "SubscriptionId", "ProductId", "ModelUsageRateId", "StartTimestamp", "StartedByAppUserId", "EndTimestamp", "EndedByAppUserId", "BillingDate", "RolloverAt", "IdempotencyKey", "BillingTimeZoneId", "Status", "HoursBilled", "Tokens", "UnitPrice", "UnitCost", "ActualCost", "UnitTypeId", "DiscountPercent", "Extended", "VendorUsageKey", "Quantity", "ResourceId", "ResourceName", "Notes", "RollupType"
        };

        private readonly IUserAdminSettings _settings;

        public ProvisionalEnvironmentArchiveStore(IUserAdminSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<ProvisionalEnvironmentArchiveWriteResult> WriteAndVerifyAsync(ProvisionalEnvironmentArchiveWriteRequest request)
        {
            if (request?.Manifest == null) throw new ArgumentNullException(nameof(request));
            if (String.IsNullOrWhiteSpace(request.Manifest.ProvisionalEnvironmentId)) throw new ArgumentException("ProvisionalEnvironmentId is required.", nameof(request));

            var billingEvents = request.BillingEvents ?? Array.Empty<ProvisionalEnvironmentBillingEventArchive>();
            var archivePath = CreateArchivePath(request.Manifest.EstablishedUtc, request.Manifest.ProvisionalEnvironmentId);
            var manifestBlobName = $"{archivePath}/{ManifestFileName}";
            var billingEventsBlobName = $"{archivePath}/{request.Manifest.BillingEventsFileName}";
            var container = await GetContainerAsync();
            var manifestBlob = container.GetBlobClient(manifestBlobName);

            if (await manifestBlob.ExistsAsync())
            {
                var existingManifest = JsonSerializer.Deserialize<ProvisionalEnvironmentArchiveManifest>((await manifestBlob.DownloadContentAsync()).Value.Content.ToString());
                if (existingManifest != null && String.Equals(existingManifest.ProvisionalEnvironmentId, request.Manifest.ProvisionalEnvironmentId, StringComparison.Ordinal))
                {
                    await VerifyBillingEventsAsync(container, billingEventsBlobName, existingManifest.BillingEventsSha256);
                    return ToResult(archivePath, manifestBlobName, billingEventsBlobName, existingManifest, true);
                }
            }

            var csv = SerializeBillingEvents(billingEvents);
            var csvBytes = Encoding.UTF8.GetBytes(csv);
            request.Manifest.ArchivedUtc = request.Manifest.ArchivedUtc == default(DateTime) ? DateTime.UtcNow : request.Manifest.ArchivedUtc.ToUniversalTime();
            request.Manifest.BillingEventCount = billingEvents.Count;
            request.Manifest.BillingEventsSha256 = ComputeSha256(csvBytes);

            var billingEventsBlob = container.GetBlobClient(billingEventsBlobName);
            await billingEventsBlob.UploadAsync(new BinaryData(csvBytes), true);

            var manifestJson = JsonSerializer.Serialize(request.Manifest, new JsonSerializerOptions { WriteIndented = true });
            await manifestBlob.UploadAsync(BinaryData.FromString(manifestJson), true);

            await VerifyBillingEventsAsync(container, billingEventsBlobName, request.Manifest.BillingEventsSha256);
            var persistedManifest = JsonSerializer.Deserialize<ProvisionalEnvironmentArchiveManifest>((await manifestBlob.DownloadContentAsync()).Value.Content.ToString());
            if (persistedManifest == null || !String.Equals(persistedManifest.ProvisionalEnvironmentId, request.Manifest.ProvisionalEnvironmentId, StringComparison.Ordinal)) throw new InvalidOperationException("The provisional environment archive manifest could not be verified.");

            return ToResult(archivePath, manifestBlobName, billingEventsBlobName, persistedManifest, false);
        }

        private async Task<BlobContainerClient> GetContainerAsync()
        {
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={_settings.UserTableStorage.AccountId};AccountKey={_settings.UserTableStorage.AccessKey}";
            var container = new BlobServiceClient(connectionString).GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        private static async Task VerifyBillingEventsAsync(BlobContainerClient container, string blobName, string expectedSha256)
        {
            if (String.IsNullOrWhiteSpace(expectedSha256)) throw new InvalidOperationException("The archive manifest does not contain a billing-events hash.");
            var content = (await container.GetBlobClient(blobName).DownloadContentAsync()).Value.Content.ToArray();
            if (!String.Equals(ComputeSha256(content), expectedSha256, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("The archived billing-events CSV failed SHA-256 verification.");
        }

        private static ProvisionalEnvironmentArchiveWriteResult ToResult(string archivePath, string manifestBlobName, string billingEventsBlobName, ProvisionalEnvironmentArchiveManifest manifest, bool alreadyExisted)
        {
            return new ProvisionalEnvironmentArchiveWriteResult
            {
                ArchivePath = archivePath,
                ManifestBlobName = manifestBlobName,
                BillingEventsBlobName = billingEventsBlobName,
                BillingEventsSha256 = manifest.BillingEventsSha256,
                BillingEventCount = manifest.BillingEventCount,
                ArchivedUtc = manifest.ArchivedUtc,
                AlreadyExisted = alreadyExisted
            };
        }

        private static string CreateArchivePath(DateTime establishedUtc, string provisionalEnvironmentId)
        {
            var utc = establishedUtc.ToUniversalTime();
            return $"{utc:yyyy}/{utc:MM}/{utc:dd}/{utc:yyyyMMddTHHmmssfffZ}-{provisionalEnvironmentId}";
        }

        private static string SerializeBillingEvents(IEnumerable<ProvisionalEnvironmentBillingEventArchive> billingEvents)
        {
            var builder = new StringBuilder();
            builder.AppendLine(String.Join(",", BillingEventColumns));
            foreach (var item in billingEvents)
            {
                builder.AppendLine(String.Join(",", new[]
                {
                    Csv(item.Id), Csv(item.SubscriptionId), Csv(item.ProductId), Csv(item.ModelUsageRateId), Csv(Format(item.StartTimestamp)), Csv(item.StartedByAppUserId), Csv(Format(item.EndTimestamp)), Csv(item.EndedByAppUserId), Csv(item.BillingDate), Csv(Format(item.RolloverAt)), Csv(item.IdempotencyKey), Csv(item.BillingTimeZoneId), Csv(item.Status), Csv(item.HoursBilled), Csv(item.Tokens), Csv(item.UnitPrice), Csv(item.UnitCost), Csv(item.ActualCost), Csv(item.UnitTypeId), Csv(item.DiscountPercent), Csv(item.Extended), Csv(item.VendorUsageKey), Csv(item.Quantity), Csv(item.ResourceId), Csv(item.ResourceName), Csv(item.Notes), Csv(item.RollupType)
                }));
            }

            return builder.ToString();
        }

        private static string Csv(object value)
        {
            if (value == null) return String.Empty;
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? String.Empty;
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }

        private static string Format(DateTime value)
        {
            return value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        private static string Format(DateTime? value)
        {
            return value.HasValue ? Format(value.Value) : null;
        }

        private static string ComputeSha256(byte[] content)
        {
            using var sha256 = SHA256.Create();
            return String.Concat(sha256.ComputeHash(content).Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
