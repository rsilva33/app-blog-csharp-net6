using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Categories;

public class EditorCategoryViewModel
{
    [Required(ErrorMessage = "The name is required.")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "This field must contain between 3 and 40 characters.")]
    public string Name { get; set; }
    [Required]
    public string Slug { get; set; }
}
