// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 29083271c55d5654fb10a861cb294a98eb7e2f437503f1b83839c0e19fa4e1f3
// IndexVersion: 2
// --- END CODE INDEX META ---
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
