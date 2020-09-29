using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IOrgUtils
    {
        Task<InvokeResult<string>> GetOrgNamespaceAsync(string orgId);
    }
}
