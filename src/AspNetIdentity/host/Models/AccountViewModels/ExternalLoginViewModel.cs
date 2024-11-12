using System.ComponentModel.DataAnnotations;

namespace Zen.IdentityServer.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
