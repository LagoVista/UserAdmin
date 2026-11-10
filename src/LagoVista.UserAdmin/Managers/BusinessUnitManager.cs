using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;

namespace LagoVista.UserAdmin.Managers
{
    public class BusinessUnitManager : ManagerBase, IBusinessUnitManager
    {
        private readonly IBusinessUnitRepo _repo;

        public BusinessUnitManager(IBusinessUnitRepo repo, IDependencyManager depManager, ISecurity security,
            IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        private static void VerifyOrganization(BusinessUnit unit, EntityHeader org)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (org == null || String.IsNullOrWhiteSpace(org.Id) || unit.OwnerOrganization?.Id != org.Id)
                throw new NotAuthorizedException("Business unit must belong to the current organization.");
        }

        public async Task<InvokeResult> AddBusinessUnitAsync(BusinessUnit businessUnit, EntityHeader org, EntityHeader user)
        {
            VerifyOrganization(businessUnit, org);
            await AuthorizeAsync(businessUnit, AuthorizeActions.Create, user, org);
            ValidationCheck(businessUnit, Actions.Create);
            if (businessUnit.IsPublic)
                return InvokeResult.FromError("Business units are private to their organization.");
            if (await _repo.QueryKeyInUseAsync(businessUnit.Key, org.Id))
                return InvokeResult.FromError("A business unit with this key already exists in the organization.");
            await _repo.AddBusinessUnitAsync(businessUnit);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateBusinessUnitAsync(BusinessUnit businessUnit, EntityHeader org, EntityHeader user)
        {
            if (businessUnit == null) throw new ArgumentNullException(nameof(businessUnit));
            var stored = await _repo.GetBusinessUnitAsync(businessUnit.Id);
            VerifyOrganization(stored, org);
            await AuthorizeAsync(stored, AuthorizeActions.Update, user, org);
            VerifyOrganization(businessUnit, org);
            ValidationCheck(businessUnit, Actions.Update);
            if (businessUnit.IsPublic)
                return InvokeResult.FromError("Business units are private to their organization.");
            if (await _repo.QueryKeyInUseAsync(businessUnit.Key, org.Id, businessUnit.Id))
                return InvokeResult.FromError("A business unit with this key already exists in the organization.");
            await _repo.UpdateBusinessUnitAsync(businessUnit);
            return InvokeResult.Success;
        }

        public async Task<BusinessUnit> GetBusinessUnitAsync(string id, EntityHeader org, EntityHeader user)
        {
            var businessUnit = await _repo.GetBusinessUnitAsync(id);
            await AuthorizeAsync(businessUnit, AuthorizeActions.Read, user, org);
            return businessUnit;
        }

        public async Task<ListResponse<BusinessUnitSummary>> GetBusinessUnitsForOrgAsync(EntityHeader org,
            EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(BusinessUnit));
            return await _repo.GetBusinessUnitsForOrgAsync(org.Id, listRequest);
        }

        public async Task<bool> QueryKeyInUseAsync(string key, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(BusinessUnit));
            return await _repo.QueryKeyInUseAsync(key, org.Id);
        }

        public async Task<DependentObjectCheckResult> CheckBusinessUnitInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var businessUnit = await _repo.GetBusinessUnitAsync(id);
            await AuthorizeAsync(businessUnit, AuthorizeActions.Read, user, org);
            return await CheckForDepenenciesAsync(businessUnit);
        }

        public async Task<InvokeResult> DeleteBusinessUnitAsync(string id, EntityHeader org, EntityHeader user)
        {
            var businessUnit = await _repo.GetBusinessUnitAsync(id);
            await AuthorizeAsync(businessUnit, AuthorizeActions.Delete, user, org);
            await ConfirmNoDepenenciesAsync(businessUnit);
            await _repo.DeleteBusinessUnitAsync(id);
            return InvokeResult.Success;
        }
    }
}
