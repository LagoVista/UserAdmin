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
