// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 56b403a04055d7b68f14c3ee6293b0518bbcbc66ae8585e0e82a0dfe0f995a8a
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LagoVista.UserAdmin.Repos.RDBMS.Models
{
    public class RDBMSAppUser
    {
        [Key]
        public string AppUserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime CreationDate { get; set; } 
        public DateTime LastUpdatedDate { get; set; }
    }

    public class RDBMSDeviceOwnerUser
    {
        [Key]
        public string DeviceOwnerUserId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }

    public class OwnedDevice
    {
        [Key]
        public string Id { get; set; }
        public string DeviceUniqueId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceOwnerUserId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Discount { get; set; }
    }
}
