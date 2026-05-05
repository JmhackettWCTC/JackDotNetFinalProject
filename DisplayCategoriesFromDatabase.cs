using Microsoft.Data.SqlClient;
using NLog;

namespace JackNETFinalProject;

public class DisplayCategoriesFromDatabase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void DisplayMenu()
    {
        Logger.Info("User entered Display Categories menu");

        Console.Clear();
        Console.WriteLine("=======================");
        Console.WriteLine("  Display Categories");
        Console.WriteLine("=======================");
        Console.WriteLine("1) All categories");
        Console.WriteLine("2) All categories with active products");
        Console.WriteLine("3) Specific category with active products");
        Console.Write("Choose option: ");

        if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 3)
        {
            Console.WriteLine("✗ Invalid choice.");
            Logger.Warn("Display categories failed: Invalid choice provided");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        if (choice == 1)
            DisplayAllCategories();
        else if (choice == 2)
            DisplayCategoriesWithProducts();
        else
            DisplaySpecificCategory();
    }

    private void DisplayAllCategories()
    {
        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT CategoryID, CategoryName, Description FROM Categories ORDER BY CategoryName";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.Clear();
                    Console.WriteLine("--- All Categories ---");
                    int count = 0;
                    while (reader.Read())
                    {
                        int id = (int)reader["CategoryID"];
                        string name = reader["CategoryName"]?.ToString() ?? "";
                        string desc = reader["Description"] != DBNull.Value ? reader["Description"]!.ToString()! : "N/A";
                        Console.WriteLine($"{id}: {name}");
                        Console.WriteLine($"   {desc}");
                        count++;
                    }

                    if (count == 0)
                        Console.WriteLine("(No categories found)");
                    else
                        Console.WriteLine($"\nTotal: {count} category/categories");
                }
            }
            Logger.Info("Displayed all categories");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error displaying categories: {ex.Message}");
            Logger.Error(ex, "Error displaying categories from database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    private void DisplayCategoriesWithProducts()
    {
        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT c.CategoryName, p.ProductName
                    FROM Categories c
                    LEFT JOIN Products p ON p.CategoryID = c.CategoryID AND p.Discontinued = 0
                    ORDER BY c.CategoryName, p.ProductName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.Clear();
                    Console.WriteLine("--- Categories with Active Products ---");
                    string? currentCategory = null;
                    int catCount = 0;

                    while (reader.Read())
                    {
                        string categoryName = reader["CategoryName"]?.ToString() ?? "";
                        string? productName = reader["ProductName"] != DBNull.Value ? reader["ProductName"]?.ToString() : null;

                        if (categoryName != currentCategory)
                        {
                            Console.WriteLine($"\n{categoryName}");
                            currentCategory = categoryName;
                            catCount++;
                        }

                        if (productName != null)
                            Console.WriteLine($"   - {productName}");
                        else
                            Console.WriteLine("   (no active products)");
                    }

                    if (catCount == 0)
                        Console.WriteLine("(No categories found)");
                }
            }
            Logger.Info("Displayed all categories with active products");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error displaying categories with products: {ex.Message}");
            Logger.Error(ex, "Error displaying categories with products");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    private void DisplaySpecificCategory()
    {
        Console.Write("Enter Category ID: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            Console.WriteLine("✗ Invalid Category ID.");
            Logger.Warn("Specific category display failed: Invalid Category ID");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT c.CategoryName, p.ProductName
                    FROM Categories c
                    LEFT JOIN Products p ON p.CategoryID = c.CategoryID AND p.Discontinued = 0
                    WHERE c.CategoryID = @CategoryID
                    ORDER BY p.ProductName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.Clear();
                        bool found = false;
                        string? categoryName = null;

                        while (reader.Read())
                        {
                            if (!found)
                            {
                                categoryName = reader["CategoryName"]?.ToString() ?? "";
                                Console.WriteLine($"--- {categoryName} ---");
                                found = true;
                            }

                            string? productName = reader["ProductName"] != DBNull.Value ? reader["ProductName"]?.ToString() : null;
                            if (productName != null)
                                Console.WriteLine($"   - {productName}");
                            else
                                Console.WriteLine("   (no active products)");
                        }

                        if (!found)
                        {
                            Console.WriteLine("✗ Category not found.");
                            Logger.Warn($"Specific category display: Category ID {categoryId} not found");
                        }
                        else
                        {
                            Logger.Info($"Displayed specific category ID {categoryId}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error displaying category: {ex.Message}");
            Logger.Error(ex, "Error displaying specific category from database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}
