using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using System;

namespace LagoVista.UserAdmin.Repos
{
    public interface IOrgIdentityData
    {
        IOrganizationRepo OrganizationRepo { get; }
        IAppUserRepo AppUserRepo { get; }
        IOrgUserRepo OrgUserRepo { get; }
    }

    public sealed class OrgIdentityData : IOrgIdentityData
    {
        public OrgIdentityData(IOrganizationRepo organizationRepo, IAppUserRepo appUserRepo, IOrgUserRepo orgUserRepo)
        {
            OrganizationRepo = organizationRepo ?? throw new ArgumentNullException(nameof(organizationRepo));
            AppUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            OrgUserRepo = orgUserRepo ?? throw new ArgumentNullException(nameof(orgUserRepo));
        }

        public IOrganizationRepo OrganizationRepo { get; }
        public IAppUserRepo AppUserRepo { get; }
        public IOrgUserRepo OrgUserRepo { get; }
    }
}
