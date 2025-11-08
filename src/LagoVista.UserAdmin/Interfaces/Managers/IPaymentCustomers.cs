// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 96326fe59337ac4644ad60589513e4baab5a6764084739cad25840fa606b1778
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    /*
     * We need to create a customer within Strip so we can place recurring payments, this will create that customer
     * and if successful will return the customer id to be added to a subscription.
     */
    public interface IPaymentCustomers
    {
        Task<InvokeResult<string>> CreateCustomerAsync(string subscriptionId, string paymentSourceId);

        Task<InvokeResult> AddPaymentSource(string customerId, string paymentSourceId);
    }
}
