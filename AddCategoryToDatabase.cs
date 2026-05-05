using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using NLog;

namespace JackNETFinalProject;

class AddCategoryToDatabase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void AddMenu()
    {
        Logger.Info("User entered Add Category menu");

        Console.Clear();
        Console.WriteLine("======================");
        Console.WriteLine("  Add Category");
        Console.WriteLine("======================");

        Console.Write("Enter Category Name: ");
        string? categoryName = Console.ReadLine();

        Console.Write("Enter Description (optional, press Enter to skip): ");
        string? description = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(description)) description = null;

        var category = new Category
        {
            CategoryName = categoryName ?? "",
            Description = description
        };

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(category, new ValidationContext(category), validationResults, true))
        {
            foreach (var result in validationResults)
                Console.WriteLine($"✗ {result.ErrorMessage}");
            Logger.Warn("Add category failed validation: {0}", string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO Categories (CategoryName, Description) VALUES (@CategoryName, @Description)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine("\n✓ Category added successfully.");
            Logger.Info($"Category '{category.CategoryName}' added successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error adding category: {ex.Message}");
            Logger.Error(ex, "Error adding category to database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}
