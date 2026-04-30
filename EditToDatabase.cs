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

        string? productName = null;
        int? supplierId = null;
        int? categoryId = null;
        string? quantityPerUnit = null;
        decimal? unitPrice = null;
        short? unitsInStock = null;
        short? unitsOnOrder = null;
        short? reorderLevel = null;
        bool discontinued = false;

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
                            productName = reader["ProductName"]?.ToString();
                            supplierId = reader["SupplierID"] as int?;
                            categoryId = reader["CategoryID"] as int?;
                            quantityPerUnit = reader["QuantityPerUnit"]?.ToString();
                            unitPrice = reader["UnitPrice"] as decimal?;
                            unitsInStock = reader["UnitsInStock"] as short?;
                            unitsOnOrder = reader["UnitsOnOrder"] as short?;
                            reorderLevel = reader["ReorderLevel"] as short?;
                            discontinued = Convert.ToBoolean(reader["Discontinued"]);
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

        Console.Write($"Product Name [{productName}]: ");
        string? newName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newName)) productName = newName;

        Console.Write($"Supplier ID [{supplierId?.ToString() ?? "null"}]: ");
        string? newSupplierInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newSupplierInput) && int.TryParse(newSupplierInput, out int newSid))
            supplierId = newSid;

        Console.Write($"Category ID [{categoryId?.ToString() ?? "null"}]: ");
        string? newCategoryInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newCategoryInput) && int.TryParse(newCategoryInput, out int newCid))
            categoryId = newCid;

        Console.Write($"Quantity Per Unit [{quantityPerUnit ?? "null"}]: ");
        string? newQtyPerUnit = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newQtyPerUnit)) quantityPerUnit = newQtyPerUnit;

        Console.Write($"Unit Price [{unitPrice?.ToString() ?? "null"}]: ");
        string? newPriceInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newPriceInput) && decimal.TryParse(newPriceInput, out decimal newUp))
            unitPrice = newUp;

        Console.Write($"Units In Stock [{unitsInStock?.ToString() ?? "null"}]: ");
        string? newStockInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newStockInput) && short.TryParse(newStockInput, out short newUis))
            unitsInStock = newUis;

        Console.Write($"Units On Order [{unitsOnOrder?.ToString() ?? "null"}]: ");
        string? newOrderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newOrderInput) && short.TryParse(newOrderInput, out short newUoo))
            unitsOnOrder = newUoo;

        Console.Write($"Reorder Level [{reorderLevel?.ToString() ?? "null"}]: ");
        string? newReorderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newReorderInput) && short.TryParse(newReorderInput, out short newRl))
            reorderLevel = newRl;

        Console.Write($"Is Discontinued? [{(discontinued ? "y" : "n")}]: ");
        string? discInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(discInput))
            discontinued = discInput.ToLower() == "y";

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
                    cmd.Parameters.AddWithValue("@ProductName", productName);
                    cmd.Parameters.AddWithValue("@SupplierID", supplierId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@QuantityPerUnit", quantityPerUnit ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitPrice", unitPrice ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitsInStock", unitsInStock ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitsOnOrder", unitsOnOrder ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReorderLevel", reorderLevel ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Discontinued", discontinued);

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
