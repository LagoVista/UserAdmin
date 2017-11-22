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
