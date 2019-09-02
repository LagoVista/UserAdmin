using LagoVista.Core.Models;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IOrgInitializer
    {
        Task Init(EntityHeader org, EntityHeader user, bool populateSampleData);
    }
}
