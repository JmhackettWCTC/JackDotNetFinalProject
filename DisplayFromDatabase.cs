using Microsoft.Data.SqlClient;
using NLog;

namespace JackNETFinalProject;

public class DisplayFromDatabase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void DisplayMenu()
    {
        Logger.Info("User entered Display menu");

        Console.Clear();
        Console.WriteLine("================");
        Console.WriteLine("  Display Menu");
        Console.WriteLine("================");
        Console.WriteLine("1) All products");
        Console.WriteLine("2) Discontinued products");
        Console.WriteLine("3) Active (not discontinued) products");
        Console.WriteLine("4) Display specific product by ID");
        Console.Write("Choose option: ");

        if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 4)
        {
            Console.WriteLine("✗ Invalid choice.");
            Logger.Warn("Display failed: Invalid choice provided");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        if (choice == 4)
        {
            DisplaySpecificProduct();
        }
        else
        {
            DisplayProductList(choice);
        }
    }

    private void DisplayProductList(int option)
    {
        string whereClause = "";
        string optionName = "";

        switch (option)
        {
            case 1:
                optionName = "All products";
                break;
            case 2:
                whereClause = "WHERE Discontinued = 1";
                optionName = "Discontinued products";
                break;
            case 3:
                whereClause = "WHERE Discontinued = 0";
                optionName = "Active products";
                break;
        }

        try
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = $"SELECT ProductID, ProductName, Discontinued FROM Products {whereClause} ORDER BY ProductName";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.Clear();
                    Console.WriteLine($"--- {optionName} ---");
                    int count = 0;
                    while (reader.Read())
                    {
                        int productId = (int)reader["ProductID"];
                        string name = reader["ProductName"]?.ToString() ?? "";
                        bool disc = Convert.ToBoolean(reader["Discontinued"]);
                        string suffix = disc ? " [DISCONTINUED]" : " [ACTIVE]";
                        Console.WriteLine($"{productId}: {name}{suffix}");
                        count++;
                    }

                    if (count == 0)
                        Console.WriteLine("(No products found)");
                    else
                        Console.WriteLine($"\nTotal: {count} product(s)");
                }
            }
            Logger.Info($"Displayed products - Option {option} ({optionName})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error displaying products: {ex.Message}");
            Logger.Error(ex, "Error displaying products from database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    public void DisplaySpecificProduct()
    {
        Logger.Info("User requested specific product display");

        Console.Write("Enter Product ID: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("✗ Invalid Product ID.");
            Logger.Warn("Specific product display failed: Invalid Product ID");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

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
                            Console.Clear();
                            Console.WriteLine("--- Product Details ---");
                            Console.WriteLine($"Product ID:       {reader["ProductID"]}");
                            Console.WriteLine($"Product Name:     {reader["ProductName"]}");
                            Console.WriteLine($"Supplier ID:      {(reader["SupplierID"] != DBNull.Value ? reader["SupplierID"] : "N/A")}");
                            Console.WriteLine($"Category ID:      {(reader["CategoryID"] != DBNull.Value ? reader["CategoryID"] : "N/A")}");
                            Console.WriteLine($"Quantity/Unit:    {(reader["QuantityPerUnit"] != DBNull.Value ? reader["QuantityPerUnit"] : "N/A")}");
                            Console.WriteLine($"Unit Price:       {(reader["UnitPrice"] != DBNull.Value ? reader["UnitPrice"] : "N/A")}");
                            Console.WriteLine($"Units In Stock:   {(reader["UnitsInStock"] != DBNull.Value ? reader["UnitsInStock"] : "N/A")}");
                            Console.WriteLine($"Units On Order:   {(reader["UnitsOnOrder"] != DBNull.Value ? reader["UnitsOnOrder"] : "N/A")}");
                            Console.WriteLine($"Reorder Level:    {(reader["ReorderLevel"] != DBNull.Value ? reader["ReorderLevel"] : "N/A")}");
                            Console.WriteLine($"Discontinued:     {(Convert.ToBoolean(reader["Discontinued"]) ? "Yes" : "No")}");
                        }
                        else
                        {
                            Console.WriteLine("✗ Product not found.");
                            Logger.Warn($"Specific product display: Product ID {productId} not found");
                        }
                    }
                }
            }
            Logger.Info($"Displayed specific product ID {productId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error displaying product: {ex.Message}");
            Logger.Error(ex, "Error displaying specific product from database");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}
