// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 319d883348ec27aee05d067bd7d111f8f9411aac37e24ab6450b12206b732624
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Exceptions;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.Repos.Security;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Users
{
    public class AppUserLoadRepo : DocumentDBRepoBase<AppUser>, IAppUserLoaderRepo
    {
        private readonly bool _shouldConsolidateCollections;
        private readonly IUserRoleRepo _userRoleRepo;

        public AppUserLoadRepo(IUserAdminSettings settings, IUserRoleRepo userRoleRepo, IAdminLogger logger, IAppConfig appConfig, ICacheProvider cacheProvider) : 
            base(settings.UserStorage.Uri, settings.UserStorage.AccessKey, settings.UserStorage.ResourceName, logger, appConfig.Environment == Environments.Development ? null : cacheProvider)
        {
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public async Task<AppUser> FindByIdAsync(string userId)
        {
            var appUser = await GetDocumentAsync(userId, false);
            if (appUser == null)
                return null;

            if (appUser.CurrentOrganization != null)
            {
                var userRoles = await _userRoleRepo.GetRolesForUserAsync(userId, appUser.CurrentOrganization.Id);
                appUser.CurrentOrganizationRoles = userRoles.Select(role => role.ToEntityHeader()).ToList();
            }

            return appUser;
        }

    }

    public class AppUserRepo : DocumentDBRepoBase<AppUser>, IAppUserRepo
    {
        private readonly bool _shouldConsolidateCollections;
        private readonly IRDBMSManager _rdbmsUserManager;
        private readonly IUserAdminSettings _adminSettings;
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly ICacheProvider _cacheProvider;
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly IAdminLogger _adminLogger;

        public AppUserRepo(IRDBMSManager rdbmsUserManager, IUserRoleRepo userRoleRepo, IUserAdminSettings userAdminSettings,  IAuthenticationLogManager authLogMgr, IAdminLogger logger, ICacheProvider cacheProvider) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _adminSettings = userAdminSettings;
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            _rdbmsUserManager = rdbmsUserManager;
            _userRoleRepo = userRoleRepo;
            _adminLogger = logger;
            _cacheProvider = cacheProvider;
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public async Task CreateAsync(AppUser user)
        {
            await CreateDocumentAsync(user);
        }

        public async Task DeleteAsync(AppUser user, bool softDelete = true)
        {
            try
            {
                await DeleteDocumentAsync(user.Id, softDelete);
                if (user.Organizations != null)
                {
                    foreach (var org in user.Organizations)
                        await _rdbmsUserManager.RemoveAppUserFromOrgAsync(org.Id, user.Id);
                }

                if (_cacheProvider != null)
                {
                    await _cacheProvider.RemoveAsync(UserNameCacheKey(user.UserName));
                    if(!String.IsNullOrEmpty(user.Email))
                        await _cacheProvider.RemoveAsync(EmailCacheKey(user.Email));
                }
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.Append(ex.Message);
                if (ex.InnerException != null)
                    msg.Append($";{ex.InnerException.Message}");

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.DeleteUserFailed, user, errors: msg.ToString());

                throw;
            }
        }

        /// <summary>
        /// This will not populate the current organization roles.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AppUser> GetCachedAppUserAsync(string id)
        {
            return await GetDocumentAsync(id, false);
        }

        public async Task<AppUser> FindByIdAsync(string id)
        {
            var appUser = await GetDocumentAsync(id, false);
            if (appUser == null)
                return null;
            
            if( appUser.CurrentOrganization != null)
            {
                var userRoles = await _userRoleRepo.GetRolesForUserAsync(id, appUser.CurrentOrganization.Id);
                appUser.CurrentOrganizationRoles = userRoles.Select(role => role.ToEntityHeader()).ToList();
            }

            return appUser; 
        }

        private string UserNameCacheKey(string userName)
        {
            return $"useraccountid_by_username_{userName.ToLower().Replace("@", String.Empty)}";
        }

        public async Task<AppUser> FindByNameAsync(string userName)
        {
            var sw = Stopwatch.StartNew();
            
            if (String.IsNullOrEmpty(userName))
            {
                throw new InvalidOperationException("Attempt to find user by user name, but null or empty user name provided.");
            }

            var accountId = await _cacheProvider.GetAsync(UserNameCacheKey(userName));
            if (!String.IsNullOrEmpty(accountId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByNameAsync]", $"[AppUserRepo__FindByNameAsync] - resolved {userName} for {accountId} in cache - {sw.Elapsed.TotalMilliseconds} ms");
            }
            else
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByNameAsync]", $"[AppUserRepo__FindByNameAsync] - did not {userName} in cache - {sw.Elapsed.TotalMilliseconds} ms");
                sw.Restart();
                var user = (await QueryAsync(usr => usr.UserName == userName.ToUpper())).FirstOrDefault();
                if (user == null)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Warning, "[AppUserRepo__FindByNameAsync]", $"[AppUserRepo__FindByNameAsync] - Could not find user by username {userName} - {sw.Elapsed.TotalMilliseconds} ms");
                    return null;
                }
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByNameAsync]", $"[AppUserRepo__FindByNameAsync] - Found user {userName} in storage by user name - {sw.Elapsed.TotalMilliseconds} ms");
                accountId = user.Id;
                sw.Restart();
                await _cacheProvider.AddAsync(UserNameCacheKey(userName), accountId);
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByNameAsync]", $"[AppUserRepo__FindByNameAsync] - Added user {userName} to cache for account id {accountId} - {sw.Elapsed.TotalMilliseconds} ms");
            }
            sw.Restart();

            var appUser = await FindByIdAsync(accountId);
            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByNameAsync]", $"[AppUserRepo__FindByNameAsync] - Get user by id {accountId}, {userName} from storage by id - {sw.Elapsed.TotalMilliseconds} ms");
            return appUser;
        }

        private string EmailCacheKey(String email)
        {
            return $"useraccountid_by_email_{email.ToLower().Replace("@",String.Empty)}";
        }

        public async Task<AppUser> FindByEmailAsync(string email)
        {
            var sw = Stopwatch.StartNew();

            if (String.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("Attempt to find user by email, but null or empty email provided.");
            }

            var accountId = await _cacheProvider.GetAsync(EmailCacheKey(email));
            if(!String.IsNullOrEmpty(accountId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByEmailAsync]", $"[AppUserRepo__FindByEmailAsync] - resolved user {email} to {accountId} cache in cache - {sw.Elapsed.TotalMilliseconds} ms");
            }
            else
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByEmailAsync]", $"[AppUserRepo__FindByEmailAsync] - did not {email} to an account id in storage - {sw.Elapsed.TotalMilliseconds} ms");
                sw.Restart();
                var user = (await QueryAsync(usr => usr.Email == email.ToUpper())).FirstOrDefault();
                if (user == null)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Warning, "[AppUserRepo__FindByEmailAsync]", $"[AppUserRepo__FindByEmailAsync] - Could not find user by email {email} - {sw.Elapsed.TotalMilliseconds} ms");
                    return null;
                }
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByEmailAsync]", $"[AppUserRepo__FindByEmailAsync] - Found user {email} in storage by email - {sw.Elapsed.TotalMilliseconds} ms");
                accountId = user.Id;
                sw.Restart();
                await _cacheProvider.AddAsync(EmailCacheKey(email), accountId);
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByEmailAsync]", $"[AppUserRepo__FindByEmailAsync] - Added user {email} to cache for account id {accountId} - {sw.Elapsed.TotalMilliseconds} ms");
            }
            sw.Restart();
           
            var appUser = await FindByIdAsync(accountId);
            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AppUserRepo__FindByEmailAsync]", $"[AppUserRepo__FindByEmailAsync] - Get user by id {accountId}, {email} from storage by id - {sw.Elapsed.TotalMilliseconds} ms");
            return appUser;
        }

        public async Task UpdateAsync(AppUser user)
        {
            var existingUser = await GetDocumentAsync(user.Id);
            await UpsertDocumentAsync(user);

            if (existingUser.Name != user.Name || existingUser.Email != user.Email)
            {
                foreach(var org in user.Organizations)
                    await _rdbmsUserManager.UpdateAppUserAsync(org.Id, user);
            }
        }

        public Task<AppUser> FindByThirdPartyLogin(string providerId, string providerKey)
        {
            throw new NotImplementedException();
        }

        public async Task<ListResponse<UserInfoSummary>> GetDeviceUsersAsync(string deviceRepoId, ListRequest listRequest)
        {
            return await QuerySummaryAsync<UserInfoSummary, AppUser>(usr => usr.IsUserDevice == true && usr.DeviceRepo != null && usr.DeviceRepo.Id == deviceRepoId, usr=>usr.Name, listRequest);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public async Task<IEnumerable<UserInfoSummary>> GetUserSummaryForListAsync(IEnumerable<OrgUser> orgUsers, bool useCache = true)
        {
            var sqlParams = string.Empty;
            var idx = 0;
            var parameters  = new List<QueryParameter>();
            var userIds = String.Empty;
            foreach (var orgUser in orgUsers)
            {
                if (!String.IsNullOrEmpty(sqlParams))
                {
                    sqlParams += ",";
                }
                var paramName = $"@userId{idx++}";

                sqlParams += paramName;
                parameters.Add(new QueryParameter(paramName, orgUser.UserId));
                userIds += orgUser.UserId;
            }

            var sw = Stopwatch.StartNew();
            

            using (var md5 = MD5.Create())
            {
                sqlParams.TrimEnd(',');
                var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(userIds);
                var hash = md5.ComputeHash(buffer);
                var key = ByteArrayToString(hash);
                var json = await _cacheProvider.GetAsync(key);
                if (!String.IsNullOrEmpty(json) && useCache)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<UserInfoSummary>>(json);
                }
               else
                {
                    //TODO: This seems kind of ugly...need to put more thought into this, this shouldn't be a query that is hit very often
                    var query = $"SELECT * FROM c where c.id in ({sqlParams})";

                    /* this sorta sux, but oh well */
                    var appUsers = await QueryAsync(query, parameters.ToArray());
                    var userSummaries = from appUser
                                        in appUsers
                                        join orgUser
                                        in orgUsers on appUser.Id equals orgUser.UserId
                                        select appUser.CreateSummary(orgUser.IsOrgAdmin, orgUser.IsAppBuilder);

                    json = JsonConvert.SerializeObject(userSummaries);
                    await _cacheProvider.AddAsync(key, json);
                    _adminLogger.Trace($"[AppUserRepo__GetUserSummaryForListAsync] - Cache Miss, get and process all users in {sw.Elapsed.TotalMilliseconds}ms");
                    return userSummaries;
                }
            }
        }

        public async Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest)
        {
            return await QuerySummaryAsync<UserInfoSummary, AppUser>(us => true, us => us.Name, listRequest);
        }

        public async Task<ListResponse<UserInfoSummary>> GetActiveUsersAsync(ListRequest listRequest)
        {
            return await QuerySummaryAsync<UserInfoSummary, AppUser>(us => us.IsUserDevice == false && !us.IsAccountDisabled, us => us.Name, listRequest);
        }


        public async Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest, bool? emailConfirmed, bool? phoneConfirmed)
        {
            Expression<Func<AppUser, bool>> mthd = (exp => exp.IsUserDevice == false);

            if (emailConfirmed.HasValue)
            {
                Expression<Func<AppUser, bool>> closeExpression = (exp => exp.EmailConfirmed == emailConfirmed.Value);
                var combined = Expression.And(mthd, closeExpression);
                mthd = Expression.Lambda<Func<AppUser, bool>>(combined);
            }

            if (phoneConfirmed.HasValue)
            {
                Expression<Func<AppUser, bool>> closeExpression = (exp => exp.PhoneNumberConfirmed == phoneConfirmed.Value);
                var combined = Expression.And(mthd, closeExpression);
                mthd = Expression.Lambda<Func<AppUser, bool>>(combined);
            }

            return await QuerySummaryAsync<UserInfoSummary, AppUser>(mthd, us => us.Name, listRequest);
        }

        public async Task DeleteAsync(string userId)
        {
            await this.DeleteDocumentAsync(userId);
            var user = await GetDocumentAsync(userId);
            foreach(var org in user.Organizations)
                await this._rdbmsUserManager.RemoveAppUserFromOrgAsync(org.Id, userId);
        }

        public async Task<ListResponse<UserInfoSummary>> GetUsersWithoutOrgsAsync(ListRequest listRequest)
        {
            return (await QuerySummaryAsync<UserInfoSummary, AppUser>(usr => (usr.Organizations == null || usr.Organizations.Count == 0), us=>us.Name, listRequest));
        }

        public async Task<AppUser> GetUserByExternalLoginAsync(ExternalLoginTypes loginType, string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException($"Attempt to find user with null or empty external login id for provider: {loginType}.");
            }

            var user = (await QueryAsync(usr => usr.ExternalLogins != null && usr.ExternalLogins.Where(ext => ext.Provider.Value == loginType && ext.Id == id || ext.UserName == id).Any())).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            //TODO: THIS SUX, when deserializing the query it auto converts to date time, we want the json string
            return await FindByIdAsync(user.Id);
        }
        public async Task<AppUser> RemoveExternalLoginAsync(string userId, string externalLoginId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("Attempt to find user with null or empty user name.");
            }

            var appUser = await FindByIdAsync(userId);
            if (appUser == null)
            {
                throw new RecordNotFoundException(nameof(AppUser), userId);
            }

            var existing = appUser.ExternalLogins.Where(exs => exs.Id == externalLoginId).FirstOrDefault();
            if(existing == null)
            {
                throw new RecordNotFoundException(nameof(ExternalLogin), externalLoginId);
            }

            appUser.ExternalLogins.Remove(existing);

            appUser.LastUpdatedBy = appUser.ToEntityHeader();
            appUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            await UpdateAsync(appUser);
            return appUser;
        }

        public async Task<ListResponse<UserInfoSummary>> SearchUsersAsync(string firstName, string lastName, string email, ListRequest listRequest)
        {
            Console.WriteLine($"----------------> Searchfor users: {firstName} {lastName} {email}");

            if (String.IsNullOrEmpty(firstName)) firstName = String.Empty;
            if (String.IsNullOrEmpty(lastName)) lastName = String.Empty;
            if (String.IsNullOrEmpty(email)) email = String.Empty;

            return await QuerySummaryAsync<UserInfoSummary, AppUser>(usr =>// ( usr.FirstName.ToLower().Contains(firstName.ToLower())),
                ((String.IsNullOrEmpty(firstName) || usr.FirstName.ToLower().StartsWith(firstName.ToLower())) &&
                (String.IsNullOrEmpty(lastName) || usr.LastName.ToLower().StartsWith(lastName.ToLower())) &&
                (String.IsNullOrEmpty(email) || usr.Email.ToLower().StartsWith(email.ToLower()))), 
                usr => usr.Name, listRequest);
        }

        public async Task<InvokeResult<long>> TryAcceptTotpTimeStepAsync(string userId, long candidateStep, bool updateLastMfaDateTimeUtc, string lastMfaDateTimeUtc)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (candidateStep <= 0) return InvokeResult<long>.FromError("invalid_candidate_step");
            if (updateLastMfaDateTimeUtc && String.IsNullOrEmpty(lastMfaDateTimeUtc)) return InvokeResult<long>.FromError("invalid_last_mfa_datetime_utc");

            var maxAttempts = 5;
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var user = await GetDocumentAsync(userId, false);
                if (user == null) return InvokeResult<long>.FromError("user_not_found");
                if (candidateStep <= user.LastTotpAcceptedTimeStep) return InvokeResult<long>.FromError("replay_detected");

                user.LastTotpAcceptedTimeStep = candidateStep;
                if (updateLastMfaDateTimeUtc) user.LastMfaDateTimeUtc = lastMfaDateTimeUtc;

                try
                {
                    await UpsertDocumentAsync(user, checkEtag: true);
                    return InvokeResult<long>.Create(candidateStep);
                }
                catch (ContentModifiedException)
                {
                    continue;
                }
            }

            return InvokeResult<long>.FromError("concurrency_conflict");
        }

        public async Task<AppUser> AssociateExternalLoginAsync(string userId, ExternalLogin external)
        {
            if (String.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("Attempt to find user with null or empty user name.");
            }

            var appUser = await FindByIdAsync(userId);
            if (appUser == null)
            {
                throw new RecordNotFoundException(nameof(AppUser), userId);
            }

            var existing = appUser.ExternalLogins.Where(exs => exs.Provider.Value == external.Provider.Value).FirstOrDefault();
            if (existing != null)
            {
                existing.Email = external.Email ?? existing.Email;
                existing.OAuthToken = external.OAuthToken ?? existing.OAuthToken;
                existing.OAuthTokenSecretId = external.OAuthTokenSecretId ?? existing.OAuthTokenSecretId;
                existing.OAuthTokenVerifierSecretId = external.OAuthTokenVerifierSecretId ?? existing.OAuthTokenVerifierSecretId;
                existing.FirstName = external.FirstName ?? existing.FirstName;
                existing.LastName = external.LastName ?? existing.LastName;
                existing.Organization = external.Organization ?? existing.Organization;

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.OAuthAppendUserLogin, userId, appUser.Name, oauthProvier: external.Provider.Text, extras: $"email: {external.Email}" );
            }
            else
            {
                appUser.ExternalLogins.Add(external);
            }

            appUser.LastUpdatedBy = appUser.ToEntityHeader();
            appUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            await UpdateAsync(appUser);
            return appUser;
        }
    }
}
