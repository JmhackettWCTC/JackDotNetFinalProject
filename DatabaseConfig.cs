using Microsoft.Extensions.Configuration;
using NLog;
namespace JackNETFinalProject;

public class DatabaseConfig
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static IConfiguration? _configuration;

    private static IConfiguration LoadConfiguration()
    {
        if (_configuration == null)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("DatabaseSettings.json")
                .Build();
        }
        return _configuration;
    }
    
    private static string PasswordCensor(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return "No Password Set";
        }
        return new string('*', password.Length);
    }
    public static void ConfigMenu()
    {
        while (true)
        {
            var config = LoadConfiguration();
            Logger.Info("User entered Database Config menu");
            Console.Clear();
            Console.WriteLine("========================");
            Console.WriteLine("  Database Config Menu");
            Console.WriteLine("========================");
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

            Console.WriteLine("--------------------------");
            Console.WriteLine("What would you like to change?");
            Console.WriteLine("--------------------------");
            Console.WriteLine("1) Server: " + config["Server"]);
            Console.WriteLine("2) Database: " + config["Database"]);
            Console.WriteLine("3) User: " + config["Username"]);
            Console.WriteLine("4) Password: " + PasswordCensor(config["Password"]));
            Console.WriteLine("--------------------------");
            Console.WriteLine("-1) Back to Main Menu");
            Console.Write("Enter choice: ");
            int choice = int.Parse(Console.ReadLine());
            if (choice == 1)
            {
                ChangeConfiguration("Server");
            }
            else if (choice == 2)
            {
                ChangeConfiguration("Database");
            }
            else if (choice == 3)
            {
                ChangeConfiguration("Username");
            }
            else if (choice == 4)
            {
                ChangeConfiguration("Password");
            }
            else if (choice == -1)
            {
                return;
            }
            else
            {
                Console.WriteLine("Invalid choice");
                Console.WriteLine("--------------------------");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }
    
    private static void ChangeConfiguration(string key)
    {
     Console.Clear();
     Console.WriteLine("========================");
     Console.WriteLine("  Change " + key);
     Console.WriteLine("========================");
     Console.Write("Enter new " + key + ": ");
     string value = Console.ReadLine();
     if (string.IsNullOrWhiteSpace(value))
     {
         Console.WriteLine("✗ " + key + " cannot be empty.");
         Console.WriteLine("--------------------------");
         Console.WriteLine("Press any key to continue...");
         Console.ReadKey(true);
         return;
     }
     SaveConfiguration(key, value);
     
    }

    private static void SaveConfiguration(string key, string value)
    {
        _configuration[key] = value;
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseSettings.json");
        var json = System.Text.Json.JsonSerializer.Serialize(_configuration.AsEnumerable().ToDictionary(kv => kv.Key, kv => kv.Value), new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        Logger.Info($"Configuration updated: {key} changed");
        Console.WriteLine("Configuration updated successfully.");
        Console.WriteLine("--------------------------");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }


}