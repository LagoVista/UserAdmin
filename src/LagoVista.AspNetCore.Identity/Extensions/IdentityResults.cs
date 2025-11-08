// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5ece27a7b30e92817a5567c2694c19abee4caf1421e6f1f1f2fba85b121dabdd
// IndexVersion: 2
// --- END CODE INDEX META ---
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
