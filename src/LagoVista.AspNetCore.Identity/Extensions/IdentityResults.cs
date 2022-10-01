using LagoVista.Core.Validation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Core
{
    public static class IdentityResults
    {
        public static InvokeResult ToInvokeResult(this IdentityResult identityResults)
        {

            var args = new List<KeyValuePair<string, string>>();
            var errs = new List<ErrorMessage>();

            foreach (var err in identityResults.Errors)
            {
                errs.Add(new ErrorMessage(err.Code, err.Description));
            }

            return InvokeResult.FromErrors(errs.ToArray());
        }
    }
}
