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
