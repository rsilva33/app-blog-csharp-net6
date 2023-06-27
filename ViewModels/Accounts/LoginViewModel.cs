using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts;

public class LoginViewModel
{
    [Required(ErrorMessage = "The e-mail is required.")]
    [EmailAddress(ErrorMessage = "The e-mail is invalid.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "The password is required.")]
    public string Password { get; set; }

}
