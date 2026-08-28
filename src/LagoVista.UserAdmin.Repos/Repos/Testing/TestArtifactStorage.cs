using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Testing;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Testing
{
    public class TestArtifactStorage :ITestArtifactStorage
    {
        private ICloudFileStorageClient _fileStorage;
        public TestArtifactStorage(ICloudFileStorageClient fileStorage, IAdminLogger adminLogger)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        public Task<InvokeResult<byte[]>> GetArtifactAsync(string fileName)
        {
            return  _fileStorage.GetFileAsync("SomeContainer", fileName);
        }

        public async Task<string> SaveArtifactAsync(string orgId, string runId, string artifactName, string contentType, byte[] artifactData)
        {
            var now = DateTime.UtcNow;
            var fileName = $"{orgId}/{now.Year:0000}{now.Month:00}{now.Day:00}/{runId}.{artifactName}";
            await _fileStorage.AddFileAsync("SomeContainer", fileName, artifactData, contentType);
            return fileName;
        }
    }
}
