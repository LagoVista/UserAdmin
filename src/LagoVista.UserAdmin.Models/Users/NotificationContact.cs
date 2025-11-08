// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 36c84e58b05a3edfeffa611a0096839dcce20159ce0e8c6c9509d90b94ea7130
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Users
{
    public class NotificationContact
    {
        public string AppUserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<PushNotificationChannel> PushNotificationChannels {get; set;}
    }
}
