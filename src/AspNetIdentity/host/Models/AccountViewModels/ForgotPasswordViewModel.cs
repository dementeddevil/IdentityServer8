using System.ComponentModel.DataAnnotations;

namespace IdentityServer8.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
