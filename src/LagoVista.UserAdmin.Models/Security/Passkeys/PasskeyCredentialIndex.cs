namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    public class PasskeyCredentialIndex
    {
        public string UserId { get; set; }
        public string RpId { get; set; }
        public string CredentialId { get; set; }
        public string CreatedUtc { get; set; }
    }
}
