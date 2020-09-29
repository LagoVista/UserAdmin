using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class OrgUtils : IOrgUtils
    {
        IOrganizationRepo _repo;

        public OrgUtils(IOrganizationRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<InvokeResult<string>> GetOrgNamespaceAsync(string orgId)
        {
            var org = await _repo.GetOrganizationAsync(orgId);
            return InvokeResult<string>.Create(org.Namespace);
        }
    }
}
