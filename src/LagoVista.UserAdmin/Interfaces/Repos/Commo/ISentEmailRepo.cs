using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Commo;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Commo
{
    public interface ISentEmailRepo
    {
        Task AddSentEmailAsync(SentEmail sentEmail);
        Task AddEmailBodyAsync(SentEmail email, string body);
        Task<string> GetEmailBodyAsync(SentEmail email);
        Task UpdateSentEmailAsync(SentEmail sentEmail);
        Task<SentEmail> GetSentEmailAsync(string enternalMessageId);
        Task<SentEmail> GetSentEmailAsync(string orgId, string internalMessageId);
        Task<ListResponse<SentEmail>> GetSentEmailForOrgAsync(string orgId, ListRequest listRequest);
        Task<ListResponse<SentEmail>> GetIndividualSentEmailForOrgAsync(string orgId, ListRequest listRequest);
        Task<ListResponse<SentEmail>> GetSentEmailForMailerAsync(string orgId, string mailerId, ListRequest listRequest);
    }
}
