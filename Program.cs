using NLog;

namespace JackNETFinalProject;

class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    static void Main()
    {
        Logger.Info("========== Application Started ==========");
        Logger.Info("Northwind Console Application");

        AddToDatabase addDb = new AddToDatabase();
        EditToDatabase editDb = new EditToDatabase();
        DisplayFromDatabase displayDb = new DisplayFromDatabase();
        AddCategoryToDatabase addCatDb = new AddCategoryToDatabase();
        EditCategoryInDatabase editCatDb = new EditCategoryInDatabase();
        DisplayCategoriesFromDatabase displayCatDb = new DisplayCategoriesFromDatabase();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=====================");
            Console.WriteLine("  Northwind Console");
            Console.WriteLine("=====================");
            Console.Write("Database Connection: ");
            try
            {
                using var conn = DatabaseConnection.GetConnection();
                conn.Open();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Stable\n");
                Console.ResetColor();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error\n");
                Console.ResetColor();
            }

            Console.WriteLine("--- Products ---");
            Console.WriteLine("1) Add Product");
            Console.WriteLine("2) Edit Product");
            Console.WriteLine("3) Display Products");
            Console.WriteLine("--- Categories ---");
            Console.WriteLine("4) Add Category");
            Console.WriteLine("5) Edit Category");
            Console.WriteLine("6) Display Categories");
            Console.WriteLine("--- Settings ---");
            Console.WriteLine("7) Database Config");
            Console.WriteLine("----------------");
            Console.WriteLine("8) Exit");
            Console.Write("Enter your choice: ");

            try
            {
                string? input = Console.ReadLine();
                if (!int.TryParse(input, out int choice))
                {
                    Console.WriteLine("✗ Invalid input. Please enter a number between 1 and 8.");
                    Logger.Warn("Invalid menu input: non-numeric value provided");
                    Thread.Sleep(1500);
                    continue;
                }

                if (choice == 1)
                {
                    Logger.Info("User selected Add Product");
                    addDb.AddMenu();
                }
                else if (choice == 2)
                {
                    Logger.Info("User selected Edit Product");
                    editDb.EditMenu();
                }
                else if (choice == 3)
                {
                    Logger.Info("User selected Display Products");
                    displayDb.DisplayMenu();
                }
                else if (choice == 4)
                {
                    Logger.Info("User selected Add Category");
                    addCatDb.AddMenu();
                }
                else if (choice == 5)
                {
                    Logger.Info("User selected Edit Category");
                    editCatDb.EditMenu();
                }
                else if (choice == 6)
                {
                    Logger.Info("User selected Display Categories");
                    displayCatDb.DisplayMenu();
                }
                else if (choice == 7)
                {
                    Logger.Info("User selected Database Config");
                    DatabaseConfig.ConfigMenu();
                }
                else if (choice == 8)
                {
                    Logger.Info("User selected Exit");
                    Console.Clear();
                    Console.WriteLine("Thank you for using Northwind Console Application. Goodbye!");
                    Thread.Sleep(1500);
                    break;
                }
                else
                {
                    Console.WriteLine("✗ Invalid choice. Please enter a number between 1 and 8.");
                    Logger.Warn($"Invalid menu choice: {choice}");
                    Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ An unexpected error occurred: {ex.Message}");
                Logger.Error(ex, "Unexpected error in main menu");
                Thread.Sleep(1500);
            }
        }

        Logger.Info("========== Application Closed ==========");
    }
}
