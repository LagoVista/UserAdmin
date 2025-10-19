// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0e22cc3d7d92864ec7b1c740fc148fdc8d8bd5ab73b735383a1403213ef1a266
// IndexVersion: 0
// --- END CODE INDEX META ---
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
