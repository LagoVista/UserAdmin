// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8b3966b78053e2e295804e027f8159884de245e66ab9278f7ec6ef9436419e6b
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    public class TwitterRequestToken
    {
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public bool CallbackConfirmed { get; set; }
    }

    public class TwitterAccessToken : TwitterRequestToken
    {
        public string UserId { get; set; }
        public string ScreenName { get; set; }
    }
}
