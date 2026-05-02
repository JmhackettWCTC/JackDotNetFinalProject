using Microsoft.Data.SqlClient;
using NLog;

namespace JackNETFinalProject;

public class DeleteProductFromDatabase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void DeleteMenu()
    {
        Logger.Info("User entered Delete Product menu");

        Console.Clear();
        Console.WriteLine("====================");
        Console.WriteLine("  Delete Product");
        Console.WriteLine("====================");

        Console.Write("Enter Product ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("✗ Invalid Product ID.");
            Logger.Warn("Delete product failed: Invalid Product ID provided");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        // First, fetch the product to confirm it exists and display its details
        string? productName = null;
        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT ProductName FROM Products WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            productName = reader["ProductName"]?.ToString();
                        }
                        else
                        {
                            Console.WriteLine("✗ Product not found.");
                            Logger.Warn($"Delete product failed: Product ID {productId} not found");
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
            Logger.Error(ex, "Error loading product for deletion");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        // Display the product to confirm deletion
        Console.WriteLine($"\nProduct to delete: ID {productId} - {productName}");
        Console.Write("Are you sure you want to delete this product? (y/n): ");
        string? confirm = Console.ReadLine();
        if (confirm?.ToLower() != "y")
        {
            Console.WriteLine("✗ Deletion cancelled.");
            Logger.Info($"User cancelled deletion of product ID {productId}");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        // Delete the product
        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                // Note: In a real application with referential integrity constraints,
                // attempting to delete a product with related orders would fail.
                // This delete operation respects database constraints.
                string query = "DELETE FROM Products WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"\n✓ Product '{productName}' deleted successfully.");
                        Logger.Info($"Product ID {productId} ('{productName}') deleted successfully");
                    }
                    else
                    {
                        Console.WriteLine("✗ No product was deleted.");
                        Logger.Warn($"Delete product: No rows affected for product ID {productId}");
                    }
                }
            }
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            // Error 547: The DELETE, INSERT, or UPDATE statement conflicted with a FOREIGN KEY constraint
            Console.WriteLine("\n✗ Cannot delete this product because it has related orders.");
            Console.WriteLine("   To delete this product, first remove all related orders.");
            Logger.Warn($"Delete product failed: Foreign key constraint violation for product ID {productId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error deleting product: {ex.Message}");
            Logger.Error(ex, $"Error deleting product ID {productId} from database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}

