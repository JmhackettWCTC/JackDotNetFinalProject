using System.ComponentModel.DataAnnotations;

namespace JackNETFinalProject;

public class Category
{
    [Required(ErrorMessage = "Category Name is required")]
    [MaxLength(15, ErrorMessage = "Category Name cannot exceed 15 characters")]
    public string CategoryName { get; set; } = "";

    public string? Description { get; set; }
}
