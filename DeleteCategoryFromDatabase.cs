using Microsoft.Data.SqlClient;
using NLog;

namespace JackNETFinalProject;

public class DeleteCategoryFromDatabase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void DeleteMenu()
    {
        Logger.Info("User entered Delete Category menu");

        Console.Clear();
        Console.WriteLine("======================");
        Console.WriteLine("  Delete Category");
        Console.WriteLine("======================");

        Console.Write("Enter Category ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            Console.WriteLine("✗ Invalid Category ID.");
            Logger.Warn("Delete category failed: Invalid Category ID provided");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        // First, fetch the category to confirm it exists and check for orphans
        string? categoryName = null;
        int productCount = 0;

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();

                // Get category name
                string query = "SELECT CategoryName FROM Categories WHERE CategoryID = @CategoryID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoryName = reader["CategoryName"]?.ToString();
                        }
                        else
                        {
                            Console.WriteLine("✗ Category not found.");
                            Logger.Warn($"Delete category failed: Category ID {categoryId} not found");
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey(true);
                            return;
                        }
                    }
                }

                // Check for related products
                query = "SELECT COUNT(*) as ProductCount FROM Products WHERE CategoryID = @CategoryID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            productCount = (int)reader["ProductCount"];
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error loading category: {ex.Message}");
            Logger.Error(ex, "Error loading category for deletion");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        // Display the category to confirm deletion and warn about orphans
        Console.WriteLine($"\nCategory to delete: ID {categoryId} - {categoryName}");
        
        if (productCount > 0)
        {
            Console.WriteLine($"⚠ WARNING: This category has {productCount} product(s) associated with it.");
            Console.WriteLine("You have two options:");
            Console.WriteLine("1) Delete category only (products will become orphaned - set to NULL category)");
            Console.WriteLine("2) Delete category AND all associated products");
            Console.WriteLine("3) Cancel deletion");
            Console.Write("Choose option (1-3): ");

            string? optionInput = Console.ReadLine();
            if (!int.TryParse(optionInput, out int option) || option < 1 || option > 3)
            {
                Console.WriteLine("✗ Invalid option.");
                Logger.Warn("Delete category: Invalid option provided");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return;
            }

            if (option == 3)
            {
                Console.WriteLine("✗ Deletion cancelled.");
                Logger.Info($"User cancelled deletion of category ID {categoryId}");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return;
            }

            // Additional confirmation for option 2
            if (option == 2)
            {
                Console.Write($"⚠ CONFIRM: Delete {productCount} product(s) along with the category? (y/n): ");
                string? doubleConfirm = Console.ReadLine();
                if (doubleConfirm?.ToLower() != "y")
                {
                    Console.WriteLine("✗ Deletion cancelled.");
                    Logger.Info($"User cancelled deletion of category ID {categoryId} and products");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                    return;
                }
            }

            // Proceed with selected option
            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    if (option == 1)
                    {
                        // Set all products in this category to have NULL CategoryID
                        string updateQuery = "UPDATE Products SET CategoryID = NULL WHERE CategoryID = @CategoryID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                            updateCmd.ExecuteNonQuery();
                        }
                        Logger.Info($"Orphaned {productCount} products by setting their CategoryID to NULL");
                    }
                    else if (option == 2)
                    {
                        // Delete all products in this category
                        string deleteProductsQuery = "DELETE FROM Products WHERE CategoryID = @CategoryID";
                        using (SqlCommand deleteCmd = new SqlCommand(deleteProductsQuery, conn))
                        {
                            deleteCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                            int productsDeleted = deleteCmd.ExecuteNonQuery();
                            Logger.Info($"Deleted {productsDeleted} products associated with category ID {categoryId}");
                        }
                    }

                    // Delete the category
                    string deleteCategoryQuery = "DELETE FROM Categories WHERE CategoryID = @CategoryID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteCategoryQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        int rowsAffected = deleteCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            if (option == 1)
                            {
                                Console.WriteLine($"\n✓ Category '{categoryName}' deleted successfully.");
                                Console.WriteLine($"   {productCount} product(s) have been orphaned (CategoryID set to NULL)");
                                Logger.Info($"Category ID {categoryId} ('{categoryName}') deleted. {productCount} products orphaned.");
                            }
                            else if (option == 2)
                            {
                                Console.WriteLine($"\n✓ Category '{categoryName}' and {productCount} product(s) deleted successfully.");
                                Logger.Info($"Category ID {categoryId} ('{categoryName}') and {productCount} products deleted successfully");
                            }
                        }
                        else
                        {
                            Console.WriteLine("✗ No category was deleted.");
                            Logger.Warn($"Delete category: No rows affected for category ID {categoryId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error deleting category: {ex.Message}");
                Logger.Error(ex, $"Error deleting category ID {categoryId} from database");
            }
        }
        else
        {
            // No products in category, simple delete
            Console.Write("Are you sure you want to delete this category? (y/n): ");
            string? confirm = Console.ReadLine();
            if (confirm?.ToLower() != "y")
            {
                Console.WriteLine("✗ Deletion cancelled.");
                Logger.Info($"User cancelled deletion of category ID {categoryId}");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return;
            }

            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Categories WHERE CategoryID = @CategoryID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"\n✓ Category '{categoryName}' deleted successfully.");
                            Logger.Info($"Category ID {categoryId} ('{categoryName}') deleted successfully");
                        }
                        else
                        {
                            Console.WriteLine("✗ No category was deleted.");
                            Logger.Warn($"Delete category: No rows affected for category ID {categoryId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error deleting category: {ex.Message}");
                Logger.Error(ex, $"Error deleting category ID {categoryId} from database");
            }
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}


