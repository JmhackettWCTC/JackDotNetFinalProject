using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;

namespace JackNETFinalProject;

public class DatabaseConnection
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static IConfiguration? _configuration;

    /// <summary>
    /// Loads configuration from DatabaseSettings.json
    /// </summary>
    private static IConfiguration LoadConfiguration()
    {
        if (_configuration == null)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("DatabaseSettings.json")
                .Build();
        }
        return _configuration;
    }

    /// <summary>
    /// Gets the connection string from DatabaseSettings.json
    /// </summary>
    public static string GetConnectionString()
    {
        var config = LoadConfiguration();
        var host = config["Host"];
        var port = config["Port"];
        var database = config["Database"];
        var username = config["Username"];
        var password = config["Password"];

        return $"Server={host};Port={port};Database={database};User={username};Password={password};";
    }

    /// <summary>
    /// Gets a new MySql connection using settings from DatabaseSettings.json
    /// </summary>
    public static MySqlConnection GetConnection()
    {
        try
        {
            return new MySqlConnection(GetConnectionString());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to create database connection");
            throw;
        }
    }
}