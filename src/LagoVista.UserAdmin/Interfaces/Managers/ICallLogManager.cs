// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d77d932531fc7d47dec71c5b3d17d9a5158f0097f0f7edb50d02700a67e9a8fc
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Phone;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface ICallLogManager
    {
        Task<ListResponse<CallLog>> GetPhoneContactsAsync(string toPhoneNumber, ListRequest listRequest, EntityHeader org, EntityHeader user);
    }
}
