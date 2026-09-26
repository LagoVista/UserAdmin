using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Contacts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISendGridMetricsClient
    {
        Task<InvokeResult<IReadOnlyList<EmailListSendMetrics>>> GetSingleSendMetricsAsync(
            string singleSendId,
            DateTime startUtc,
            DateTime endUtc);
    }
}
