// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c6996bcab9cf9637b60b5e4729c5b33348a1d3b62c60f9fdc3ca35b17c9c8bf8
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Interfaces.Managers;
using System.Collections.Generic;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.Core.Interfaces;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Managers;
using static LagoVista.Core.Models.AuthorizeResult;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Managers
{
    public class AssetSetManager : ManagerBase, IAssetSetManager
    {
        IAssetSetRepo _assetSetRepo;
        IManagedAssetRepo _managedAssetRepo;

        public AssetSetManager(IAssetSetRepo assetSetRepo, IManagedAssetRepo managedAssetRepo, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _assetSetRepo = assetSetRepo;
            _managedAssetRepo = managedAssetRepo;
        }

        /* Working with Asset Set */
        public async Task<InvokeResult> AddAssetSetAsync(AssetSet assetSet, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(assetSet, AuthorizeActions.Create, user, org);
            ValidationCheck(assetSet, Actions.Create);
            await _assetSetRepo.AddAssetSetAsync(assetSet);
            return InvokeResult.Success;
        }

        public async Task<AssetSet> GetAssetSetAsync(string id, EntityHeader org, EntityHeader user)
        {
            var assetSet = await _assetSetRepo.GetAssetSetAsync(id);
            await AuthorizeAsync(assetSet, AuthorizeActions.Read, user, org);
            return assetSet;
        }

        public async Task<InvokeResult> UpdateAssetSetAsync(AssetSet assetSet, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(assetSet, AuthorizeActions.Update, user, org);
            ValidationCheck(assetSet, Actions.Create);
            await _assetSetRepo.UpdateAssetSetAsync(assetSet);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteAssetSetAsync(string id, EntityHeader org, EntityHeader user)
        {
            var assetSet = await _assetSetRepo.GetAssetSetAsync(id);
            await AuthorizeAsync(assetSet, AuthorizeActions.Delete, user, org);
            await ConfirmNoDepenenciesAsync(assetSet);
            await _assetSetRepo.DeleteAssetSetAsync(id);
            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var assetSet = await _assetSetRepo.GetAssetSetAsync(id);
            await AuthorizeAsync(assetSet, AuthorizeResult.AuthorizeActions.Read, user, org);
            return await CheckForDepenenciesAsync(assetSet);
        }

        public Task<bool> QueryKeyInUseAsync(string key, EntityHeader org)
        {
            return _assetSetRepo.QueryKeyInUseAsync(key, org.Id);
        }

        public async Task<IEnumerable<AssetSetSummary>> GetAssetSetsForOrgAsync(string orgId, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, orgId, typeof(AssetSet));
            return await _assetSetRepo.GetAssetSetsForOrgAsync(orgId);
        }

        /* Next set of code to manage assigned managed assets */
        public async Task<IEnumerable<AssetSetSummary>> GetAssetSetsForManagedAssetAsync(string assetId, EntityHeader org, EntityHeader user)
        {
            /* It's either all objects or none. */
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(ManagedAsset));
            return await _managedAssetRepo.GetAssetSetsForManagedAssetAsync(assetId);
        }

        public async Task<IEnumerable<ManagedAssetSummary>> GetManagedAssetsFromAssetSetAsync(string assetSetId, EntityHeader org, EntityHeader user)
        {
            var assetSet = await _assetSetRepo.GetAssetSetAsync(assetSetId);
            await AuthorizeAsync(assetSet, AuthorizeActions.Read, user, org);
            return await _managedAssetRepo.GetManagedAssetsAsync(assetSetId);            
        }

        public async Task<InvokeResult> AddManagedAssetAsync(ManagedAsset asset, EntityHeader org, EntityHeader user)
        {
            var assetSet = await _assetSetRepo.GetAssetSetAsync(asset.AssetSetId);
            await AuthorizeAsync(assetSet, AuthorizeActions.Update, user, org);
            await _managedAssetRepo.AddManagedAssetAsync(asset);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> RemoveManagedAssetAsync(string assetSetId, string assetId, EntityHeader org, EntityHeader user)
        {
            var assetSet = await _assetSetRepo.GetAssetSetAsync(assetSetId);
            await AuthorizeAsync(assetSet, AuthorizeActions.Update, user, org);
            await _managedAssetRepo.RemoveManagedAssetAsync(assetSetId, assetId);
            return InvokeResult.Success;
        }
    }
}