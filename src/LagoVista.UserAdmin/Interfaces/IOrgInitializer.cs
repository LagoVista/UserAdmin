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
