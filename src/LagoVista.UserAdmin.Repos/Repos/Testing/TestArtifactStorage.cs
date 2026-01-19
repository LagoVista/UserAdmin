using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Testing;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Testing
{
    public class TestArtifactStorage : CloudFileStorage, ITestArtifactStorage
    {
        public TestArtifactStorage(IUserAdminSettings settings, IAdminLogger adminLogger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey,"testingartifact", adminLogger)
        {
        }

        public Task<InvokeResult<byte[]>> GetArtifactAsync(string fileName)
        {
            return GetFileAsync(fileName);
        }

        public async Task<string> SaveArtifactAsync(string orgId, string runId, string artifactName, string contentType, byte[] artifactData)
        {
            var now = DateTime.UtcNow;
            var fileName = $"{orgId}/{now.Year:0000}{now.Month:00}{now.Day:00}/{runId}.{artifactName}";
            await AddFileAsync(fileName, artifactData, contentType);
            return fileName;
        }
    }
}
