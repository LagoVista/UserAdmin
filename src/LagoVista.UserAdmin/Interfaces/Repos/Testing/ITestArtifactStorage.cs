using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Testing
{
    public interface ITestArtifactStorage
    {
        Task<string> SaveArtifactAsync(string orgId, string runId, string artifactName, string contentType, byte[] artifactData);
        Task<InvokeResult<byte[]>> GetArtifactAsync(string fileName);
    }
}
