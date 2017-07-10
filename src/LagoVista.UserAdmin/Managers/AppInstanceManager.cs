using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Models.Users;
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

        public async Task<InvokeResult<AppInstance>> CreateForUserAsync(AppUser appUser, AuthRequest authRequest)
        {
            authRequest.AppInstanceId = Guid.NewGuid().ToId();
            var appInstance = new AppInstance(authRequest.AppInstanceId, appUser.Id);

            appInstance.UserId = appUser.Id;
            appInstance.AppId = authRequest.AppId;
            appInstance.DeviceId = authRequest.DeviceId;
            appInstance.ClientType = authRequest.ClientType;

            appInstance.CreationDate = DateTime.UtcNow.ToJSONString();
            appInstance.LastAccessTokenRefresh = DateTime.UtcNow.ToJSONString();
            appInstance.LastLogin = DateTime.UtcNow.ToJSONString();

            await _appInstanceRepo.AddAppInstanceAsync(appInstance);

            return InvokeResult<AppInstance>.Create(appInstance);
        }

        public async Task<InvokeResult> UpdateLastLoginAsync(string appUserId, string appInstanceId)
        {
            var appInstance = await _appInstanceRepo.GetAppInstanceAsync(appUserId, appInstanceId);
            if (appInstance == null)
            {
                _adminLogger.AddError("AppInstanceManager_UpdateLastLoginAsync", "Could Not Find App Instance Record",
                    new KeyValuePair<string, string>("appUserId", appUserId),
                    new KeyValuePair<string, string>("appInstanceId", appInstanceId));

                return InvokeResult.FromErrors(new ErrorMessage() { Message = "Could Not Find App Instance Id to Update" });
            }
            else
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "AppInstanceManager_UpdateLastLoginAsync", "Update Last Login Information",
                    new KeyValuePair<string, string>("appUserId", appUserId),
                    new KeyValuePair<string, string>("appInstanceId", appInstanceId));

                appInstance.LastLogin = DateTime.UtcNow.ToJSONString();
                await _appInstanceRepo.UpdateAppInstanceAsync(appInstance);
                return InvokeResult.Success;
            }
        }

        public async Task<InvokeResult> UpdateLastAccessTokenRefreshAsync(string appUserId, string appInstanceId)
        {
            var appInstance = await _appInstanceRepo.GetAppInstanceAsync(appUserId, appInstanceId);
            appInstance.LastAccessTokenRefresh = DateTime.UtcNow.ToJSONString();
            await _appInstanceRepo.UpdateAppInstanceAsync(appInstance);
            return InvokeResult.Success;
        }
    }
}