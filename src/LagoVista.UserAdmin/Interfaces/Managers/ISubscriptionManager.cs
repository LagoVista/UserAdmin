﻿using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISubscriptionManager
    {
        Task<InvokeResult> AddSubscriptionAsync(SubscriptionDTO subscription, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateSubscriptionAsync(SubscriptionDTO subscription, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteSubscriptionsForOrgAsync(string id, EntityHeader org, EntityHeader user);
        Task<SubscriptionDTO> GetTrialSubscriptionAsync(EntityHeader org, EntityHeader user);
        Task<SubscriptionDTO> GetSubscriptionAsync(Guid id, EntityHeader org, EntityHeader user);
        Task<ListResponse<SubscriptionResource>> GetResourcesForSubscriptionAsync(Guid subscriptionId, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId, EntityHeader user);
        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);
        bool IsForInitialization { get; set; }
    }
}
