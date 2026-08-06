namespace LagoVista.UserAdmin.Models.DTOs
{
    public class VerifyPasswordResetCode
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
