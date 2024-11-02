using System.ComponentModel.DataAnnotations;

namespace IdentityServer8.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
