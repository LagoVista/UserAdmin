// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 226ecbcc3cf49fa2fa7f9b1954d2373e170bb405583c752ce8feb8fe6ebfbddb
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IOrgInitializer
    {
        Task Init(EntityHeader org, EntityHeader user, bool populateSampleData);
        Task<InvokeResult> CreateExampleAppAsync(string environmentName, EntityHeader org, EntityHeader user);
    }
}
