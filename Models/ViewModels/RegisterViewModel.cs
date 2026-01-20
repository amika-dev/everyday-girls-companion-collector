using System.ComponentModel.DataAnnotations;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// ViewModel for user registration with password confirmation.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Could you provide your email address?")]
        [EmailAddress(ErrorMessage = "That doesn't look like a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Could you choose a password?")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Your password should be at least 6 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Could you confirm your password?")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Hmm, the passwords don't match. Could you try again?")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
