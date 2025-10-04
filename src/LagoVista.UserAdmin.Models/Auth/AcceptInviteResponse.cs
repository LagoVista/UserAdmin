using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Auth
{
    public class AcceptInviteResponse
    {
        public string RedirectPage { get; set; }
        public string ResponseMessage { get; set; }
        public EntityHeader Customer { get; set; }
        public EntityHeader CustomerContact { get; set; }
        public EntityHeader EndUserAppOrg {get; set;}
    }
}
