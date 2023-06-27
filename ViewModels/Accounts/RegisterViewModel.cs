using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts;

public class RegisterViewModel
{
    [Required(ErrorMessage = "The name is required.")]
    public string Name { get; set; }


    [Required(ErrorMessage = "The e-mail is required.")]
    [EmailAddress(ErrorMessage = "The e-mail is invalid.")]
    public string Email { get; set; }
}
