using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using NLog;

namespace JackNETFinalProject;

class AddToDatabase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void AddMenu()
    {
        Logger.Info("User entered Add menu");

        Console.Clear();
        Console.WriteLine("===================");
        Console.WriteLine("  Add To Database");
        Console.WriteLine("===================");

        Console.Write("Enter Product Name: ");
        string? productName = Console.ReadLine();

        Console.Write("Enter Supplier ID (optional, press Enter to skip): ");
        int? supplierId = null;
        string? supplierInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(supplierInput) && int.TryParse(supplierInput, out int sid))
            supplierId = sid;

        Console.Write("Enter Category ID (optional, press Enter to skip): ");
        int? categoryId = null;
        string? categoryInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(categoryInput) && int.TryParse(categoryInput, out int cid))
            categoryId = cid;

        Console.Write("Enter Quantity Per Unit (optional): ");
        string? quantityPerUnit = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(quantityPerUnit)) quantityPerUnit = null;

        Console.Write("Enter Unit Price (optional): ");
        decimal? unitPrice = null;
        string? priceInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal up))
            unitPrice = up;

        Console.Write("Enter Units In Stock (optional): ");
        short? unitsInStock = null;
        string? stockInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(stockInput) && short.TryParse(stockInput, out short uis))
            unitsInStock = uis;

        Console.Write("Enter Units On Order (optional): ");
        short? unitsOnOrder = null;
        string? orderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(orderInput) && short.TryParse(orderInput, out short uoo))
            unitsOnOrder = uoo;

        Console.Write("Enter Reorder Level (optional): ");
        short? reorderLevel = null;
        string? reorderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(reorderInput) && short.TryParse(reorderInput, out short rl))
            reorderLevel = rl;

        Console.Write("Is Discontinued? (y/n, default n): ");
        bool discontinued = Console.ReadLine()?.ToLower() == "y";

        var product = new Product
        {
            ProductName = productName ?? "",
            SupplierID = supplierId,
            CategoryID = categoryId,
            QuantityPerUnit = quantityPerUnit,
            UnitPrice = unitPrice,
            UnitsInStock = unitsInStock,
            UnitsOnOrder = unitsOnOrder,
            ReorderLevel = reorderLevel,
            Discontinued = discontinued
        };

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(product, new ValidationContext(product), validationResults, true))
        {
            foreach (var result in validationResults)
                Console.WriteLine($"✗ {result.ErrorMessage}");
            Logger.Warn("Add product failed validation: {0}", string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO Products (ProductName, SupplierID, CategoryID, QuantityPerUnit, UnitPrice, UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued)
                                 VALUES (@ProductName, @SupplierID, @CategoryID, @QuantityPerUnit, @UnitPrice, @UnitsInStock, @UnitsOnOrder, @ReorderLevel, @Discontinued)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@SupplierID", product.SupplierID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryID", product.CategoryID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@QuantityPerUnit", product.QuantityPerUnit ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitPrice", product.UnitPrice ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitsInStock", product.UnitsInStock ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitsOnOrder", product.UnitsOnOrder ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReorderLevel", product.ReorderLevel ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Discontinued", product.Discontinued);
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine("\n✓ Product added successfully.");
            Logger.Info($"Product '{product.ProductName}' added successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error adding product: {ex.Message}");
            Logger.Error(ex, "Error adding product to database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}
