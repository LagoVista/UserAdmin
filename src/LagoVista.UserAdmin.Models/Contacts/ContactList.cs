using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Contacts
{
    public class ContactList
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public int Count { get; set; }
        public string LastUpdated { get; set; }
    }
}
