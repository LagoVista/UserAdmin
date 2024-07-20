using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Users
{
    public enum Platforms
    {
        Apple,
        Android,
        Windows,
        Chrome,
    }

    public class PushNotificationChannel
    {
        public string Id { get; set; }
        public string CreationDate { get; set; }
        public string Token { get; set; }
        public Platforms Platform { get; set; }

    }
}
