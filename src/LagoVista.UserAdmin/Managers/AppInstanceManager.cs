using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using System;
using LagoVista.Core;
using System.Threading.Tasks;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Managers
{
    public class AppInstanceManager : IAppInstanceManager
    {
        IAppInstanceRepo _appInstanceRepo;
        IAdminLogger _adminLogger;

        public AppInstanceManager(IAppInstanceRepo appInstanceRepo, IAdminLogger adminLogger)
        {
            _appInstanceRepo = appInstanceRepo;
            _adminLogger = adminLogger;
        }

        public async Task<InvokeResult<AppInstance>> CreateForUserAsync(string appUserId, AuthRequest authRequest)
        {
            if(String.IsNullOrEmpty(authRequest.AppInstanceId))
                authRequest.AppInstanceId = Guid.NewGuid().ToId();

            var appInstance = new AppInstance(authRequest.AppInstanceId, appUserId);

            appInstance.UserId = appUserId;
            appInstance.AppId = authRequest.AppId;
            appInstance.DeviceId = authRequest.DeviceId;
            appInstance.ClientType = authRequest.ClientType;

            appInstance.CreationDate = DateTime.UtcNow.ToJSONString();
            appInstance.LastAccessTokenRefresh = DateTime.UtcNow.ToJSONString();
            appInstance.LastLogin = DateTime.UtcNow.ToJSONString();

            await _appInstanceRepo.AddAppInstanceAsync(appInstance);

            return InvokeResult<AppInstance>.Create(appInstance);
        }

        public async Task<InvokeResult<AppInstance>> UpdateLastLoginAsync(string appUserId, AuthRequest existingAppInstance)
        {
            var appInstance = await _appInstanceRepo.GetAppInstanceAsync(appUserId, existingAppInstance.AppInstanceId);
            if (appInstance == null)
            {
                _adminLogger.AddError("AppInstanceManager_UpdateLastLoginAsync", "Could not load, possible user id change",
                    new KeyValuePair<string, string>("appUserId", appUserId),
                    new KeyValuePair<string, string>("appInstanceId", existingAppInstance.AppInstanceId));

                return await CreateForUserAsync(appUserId, existingAppInstance);
            }
            else
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "AppInstanceManager_UpdateLastLoginAsync", "Update Last Login Information",
                    new KeyValuePair<string, string>("appUserId", appUserId),
                    new KeyValuePair<string, string>("appInstanceId", existingAppInstance.AppInstanceId));

                appInstance.LastLogin = DateTime.UtcNow.ToJSONString();
                await _appInstanceRepo.UpdateAppInstanceAsync(appInstance);
                return InvokeResult<AppInstance>.Create(appInstance);
            }
        }

        public async Task<InvokeResult<AppInstance>> UpdateLastAccessTokenRefreshAsync(string appUserId, AuthRequest existingAppInstance)
        {
            var appInstance = await _appInstanceRepo.GetAppInstanceAsync(appUserId, existingAppInstance.AppInstanceId);
            if (appInstance == null)
            {
                _adminLogger.AddError("AppInstanceManager_UpdateLastAccessTokenRefreshAsync", "Could not load, possible user id change",
                    new KeyValuePair<string, string>("appUserId", appUserId),
                    new KeyValuePair<string, string>("appInstanceId", existingAppInstance.AppInstanceId));

                return await CreateForUserAsync(appUserId, existingAppInstance);
            }
            else
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "AppInstanceManager_UpdateLastAccessTokenRefreshAsync", "Update Last Access Token Refresh",
                    new KeyValuePair<string, string>("appUserId", appUserId),
                    new KeyValuePair<string, string>("appInstanceId", existingAppInstance.AppInstanceId));

                appInstance.LastAccessTokenRefresh = DateTime.UtcNow.ToJSONString();
                await _appInstanceRepo.UpdateAppInstanceAsync(appInstance);
                return InvokeResult<AppInstance>.Create(appInstance);
            }
        }

    }
}