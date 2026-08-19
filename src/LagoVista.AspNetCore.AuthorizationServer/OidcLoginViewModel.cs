using System.ComponentModel.DataAnnotations;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class OidcLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        [Required]
        public string ReturnUrl { get; set; }
    }
}
