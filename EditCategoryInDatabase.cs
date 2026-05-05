using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using NLog;

namespace JackNETFinalProject;

public class EditCategoryInDatabase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void EditMenu()
    {
        Logger.Info("User entered Edit Category menu");

        Console.Clear();
        Console.WriteLine("======================");
        Console.WriteLine("  Edit Category");
        Console.WriteLine("======================");

        Console.Write("Enter Category ID to edit: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            Console.WriteLine("✗ Invalid Category ID.");
            Logger.Warn("Edit category failed: Invalid Category ID provided");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        var category = new Category();

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT CategoryID, CategoryName, Description FROM Categories WHERE CategoryID = @CategoryID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            category.CategoryName = reader["CategoryName"]?.ToString() ?? "";
                            category.Description = reader["Description"] != DBNull.Value ? reader["Description"]?.ToString() : null;
                        }
                        else
                        {
                            Console.WriteLine("✗ Category not found.");
                            Logger.Warn($"Edit category failed: Category ID {categoryId} not found");
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
            Console.WriteLine($"✗ Error loading category: {ex.Message}");
            Logger.Error(ex, "Error loading category for edit");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine("\nCurrent category found. Enter new values or press Enter to keep existing values.\n");

        Console.Write($"Category Name [{category.CategoryName}]: ");
        string? newName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newName)) category.CategoryName = newName;

        Console.Write($"Description [{category.Description ?? "null"}]: ");
        string? newDescription = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newDescription)) category.Description = newDescription;

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(category, new ValidationContext(category), validationResults, true))
        {
            foreach (var result in validationResults)
                Console.WriteLine($"✗ {result.ErrorMessage}");
            Logger.Warn("Edit category failed validation: {0}", string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "UPDATE Categories SET CategoryName = @CategoryName, Description = @Description WHERE CategoryID = @CategoryID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine("\n✓ Category updated successfully.");
            Logger.Info($"Category ID {categoryId} updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error updating category: {ex.Message}");
            Logger.Error(ex, "Error updating category in database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}
