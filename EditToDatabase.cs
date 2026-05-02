using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using NLog;

namespace JackNETFinalProject;

public class EditToDatabase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void EditMenu()
    {
        Logger.Info("User entered Edit menu");

        Console.Clear();
        Console.WriteLine("===================");
        Console.WriteLine("  Edit Product");
        Console.WriteLine("===================");

        Console.Write("Enter Product ID to edit: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("✗ Invalid Product ID.");
            Logger.Warn("Edit failed: Invalid Product ID provided");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        var product = new Product();

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Products WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product.ProductName = reader["ProductName"]?.ToString() ?? "";
                            product.SupplierID = reader["SupplierID"] as int?;
                            product.CategoryID = reader["CategoryID"] as int?;
                            product.QuantityPerUnit = reader["QuantityPerUnit"]?.ToString();
                            product.UnitPrice = reader["UnitPrice"] as decimal?;
                            product.UnitsInStock = reader["UnitsInStock"] as short?;
                            product.UnitsOnOrder = reader["UnitsOnOrder"] as short?;
                            product.ReorderLevel = reader["ReorderLevel"] as short?;
                            product.Discontinued = Convert.ToBoolean(reader["Discontinued"]);
                        }
                        else
                        {
                            Console.WriteLine("✗ Product not found.");
                            Logger.Warn($"Edit failed: Product ID {productId} not found");
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey(true);
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error loading product: {ex.Message}");
            Logger.Error(ex, "Error loading product for edit");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine("\nCurrent product found. Enter new values or press Enter to keep existing values.\n");

        Console.Write($"Product Name [{product.ProductName}]: ");
        string? newName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newName)) product.ProductName = newName;

        Console.Write($"Supplier ID [{product.SupplierID?.ToString() ?? "null"}]: ");
        string? newSupplierInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newSupplierInput) && int.TryParse(newSupplierInput, out int newSid))
            product.SupplierID = newSid;

        Console.Write($"Category ID [{product.CategoryID?.ToString() ?? "null"}]: ");
        string? newCategoryInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newCategoryInput) && int.TryParse(newCategoryInput, out int newCid))
            product.CategoryID = newCid;

        Console.Write($"Quantity Per Unit [{product.QuantityPerUnit ?? "null"}]: ");
        string? newQtyPerUnit = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newQtyPerUnit)) product.QuantityPerUnit = newQtyPerUnit;

        Console.Write($"Unit Price [{product.UnitPrice?.ToString() ?? "null"}]: ");
        string? newPriceInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newPriceInput) && decimal.TryParse(newPriceInput, out decimal newUp))
            product.UnitPrice = newUp;

        Console.Write($"Units In Stock [{product.UnitsInStock?.ToString() ?? "null"}]: ");
        string? newStockInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newStockInput) && short.TryParse(newStockInput, out short newUis))
            product.UnitsInStock = newUis;

        Console.Write($"Units On Order [{product.UnitsOnOrder?.ToString() ?? "null"}]: ");
        string? newOrderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newOrderInput) && short.TryParse(newOrderInput, out short newUoo))
            product.UnitsOnOrder = newUoo;

        Console.Write($"Reorder Level [{product.ReorderLevel?.ToString() ?? "null"}]: ");
        string? newReorderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newReorderInput) && short.TryParse(newReorderInput, out short newRl))
            product.ReorderLevel = newRl;

        Console.Write($"Is Discontinued? [{(product.Discontinued ? "y" : "n")}]: ");
        string? discInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(discInput))
            product.Discontinued = discInput.ToLower() == "y";

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(product, new ValidationContext(product), validationResults, true))
        {
            foreach (var result in validationResults)
                Console.WriteLine($"✗ {result.ErrorMessage}");
            Logger.Warn("Edit product failed validation: {0}", string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"UPDATE Products SET ProductName = @ProductName, SupplierID = @SupplierID, CategoryID = @CategoryID,
                                 QuantityPerUnit = @QuantityPerUnit, UnitPrice = @UnitPrice, UnitsInStock = @UnitsInStock,
                                 UnitsOnOrder = @UnitsOnOrder, ReorderLevel = @ReorderLevel, Discontinued = @Discontinued
                                 WHERE ProductID = @ProductID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
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

            Console.WriteLine("\n✓ Product updated successfully.");
            Logger.Info($"Product ID {productId} updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error updating product: {ex.Message}");
            Logger.Error(ex, "Error updating product in database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}
