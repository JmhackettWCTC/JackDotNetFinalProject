using System.ComponentModel.DataAnnotations;

namespace JackNETFinalProject;

public class Product
{
    [Required(ErrorMessage = "Product Name is required")]
    [MaxLength(40, ErrorMessage = "Product Name cannot exceed 40 characters")]
    public string ProductName { get; set; } = "";

    public int? SupplierID { get; set; }
    public int? CategoryID { get; set; }

    [MaxLength(20, ErrorMessage = "Quantity Per Unit cannot exceed 20 characters")]
    public string? QuantityPerUnit { get; set; }

    [Range(0, 9999.99, ErrorMessage = "Unit Price must be between 0 and 9999.99")]
    public decimal? UnitPrice { get; set; }

    [Range(0, 32767, ErrorMessage = "Units In Stock must be between 0 and 32767")]
    public short? UnitsInStock { get; set; }

    [Range(0, 32767, ErrorMessage = "Units On Order must be between 0 and 32767")]
    public short? UnitsOnOrder { get; set; }

    [Range(0, 32767, ErrorMessage = "Reorder Level must be between 0 and 32767")]
    public short? ReorderLevel { get; set; }

    public bool Discontinued { get; set; }
}
