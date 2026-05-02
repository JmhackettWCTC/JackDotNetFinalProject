using Microsoft.Extensions.Configuration;
using NLog;
using System.Text.Json;

namespace JackNETFinalProject;

public class DatabaseConfig
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static IConfiguration? _configuration;

    private static IConfiguration LoadConfiguration()
    {
        try
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
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading configuration from DatabaseSettings.json");
            throw;
        }
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
        try
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
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Error\n");
                    Console.ResetColor();
                    Logger.Warn(ex, "Database connection test failed in config menu");
                }

                Console.WriteLine("--------------------------");
                Console.WriteLine("What would you like to change?");
                Console.WriteLine("--------------------------");
                Console.WriteLine("1) Server: " + (config["Server"] ?? "Not Set"));
                Console.WriteLine("2) Database: " + (config["Database"] ?? "Not Set"));
                Console.WriteLine("3) User: " + (config["Username"] ?? "Not Set"));
                Console.WriteLine("4) Password: " + PasswordCensor(config["Password"] ?? ""));
                Console.WriteLine("--------------------------");
                Console.WriteLine("-1) Back to Main Menu");
                Console.Write("Enter choice: ");
                
                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("✗ Invalid input. Please enter a valid number.");
                    Logger.Warn("DatabaseConfig menu: Invalid menu input provided");
                    Console.WriteLine("--------------------------");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                    continue;
                }

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
                    Logger.Info("User exited Database Config menu");
                    return;
                }
                else
                {
                    Console.WriteLine("✗ Invalid choice. Please enter a number from the list.");
                    Logger.Warn($"DatabaseConfig menu: Invalid choice {choice}");
                    Console.WriteLine("--------------------------");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ An error occurred in Database Config: {ex.Message}");
            Logger.Error(ex, "Unexpected error in DatabaseConfig menu");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }
    
    private static void ChangeConfiguration(string key)
    {
        try
        {
            Console.Clear();
            Console.WriteLine("========================");
            Console.WriteLine("  Change " + key);
            Console.WriteLine("========================");
            Console.Write("Enter new " + key + ": ");
            string? value = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("✗ " + key + " cannot be empty.");
                Logger.Warn($"DatabaseConfig: Attempted to set empty {key}");
                Console.WriteLine("--------------------------");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return;
            }
            
            SaveConfiguration(key, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error changing {key}: {ex.Message}");
            Logger.Error(ex, $"Error changing configuration key: {key}");
            Console.WriteLine("--------------------------");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }

    private static void SaveConfiguration(string key, string value)
    {
        try
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseSettings.json");
            
            // Read the current JSON file
            if (!File.Exists(filePath))
            {
                Console.WriteLine("✗ DatabaseSettings.json not found.");
                Logger.Error($"DatabaseSettings.json not found at {filePath}");
                Console.WriteLine("--------------------------");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return;
            }

            // Read existing configuration
            string jsonContent = File.ReadAllText(filePath);
            using JsonDocument doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            // Create a mutable dictionary from the JSON
            var configDict = new Dictionary<string, string>();
            foreach (var property in root.EnumerateObject())
            {
                configDict[property.Name] = property.Value.GetString() ?? "";
            }

            // Update the key
            configDict[key] = value;

            // Serialize back to JSON with formatting
            var options = new JsonSerializerOptions { WriteIndented = true };
            string updatedJson = JsonSerializer.Serialize(configDict, options);

            // Write back to file
            File.WriteAllText(filePath, updatedJson);

            // Reset configuration so it reloads on next access
            _configuration = null;

            Logger.Info($"Configuration updated: {key} changed successfully");
            Console.WriteLine("✓ Configuration updated successfully.");
            Console.WriteLine("--------------------------");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
        catch (JsonException ex)
        {
            Console.WriteLine("✗ Error: DatabaseSettings.json is not valid JSON.");
            Logger.Error(ex, "JSON parsing error in SaveConfiguration");
            Console.WriteLine("--------------------------");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
        catch (IOException ex)
        {
            Console.WriteLine("✗ Error: Cannot write to DatabaseSettings.json (permission or file locked).");
            Logger.Error(ex, "IO error in SaveConfiguration");
            Console.WriteLine("--------------------------");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Unexpected error saving configuration: {ex.Message}");
            Logger.Error(ex, "Unexpected error in SaveConfiguration");
            Console.WriteLine("--------------------------");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }

}