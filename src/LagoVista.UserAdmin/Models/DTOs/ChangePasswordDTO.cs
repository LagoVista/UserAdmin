using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class ChangePasswordDTO
    {
        public String UserId { get; set; }
        public String OldPassword { get; set; }
        public String NewPassword { get; set; }
    }
}
