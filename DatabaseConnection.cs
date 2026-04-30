using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NLog;

namespace JackNETFinalProject;

public class DatabaseConnection
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static IConfiguration? _configuration;

    private static IConfiguration LoadConfiguration()
    {
        if (_configuration == null)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("DatabaseSettings.json", optional: true)
                .Build();
        }
        return _configuration;
    }

    public static string GetConnectionString()
    {
        var config = LoadConfiguration();
        var server = config["Server"];
        var database = config["Database"];
        var username = config["Username"];
        var password = config["Password"];

        return $"Server={server};Database={database};User Id={username};Password={password};TrustServerCertificate=True;";
    }

    public static SqlConnection GetConnection()
    {
        try
        {
            return new SqlConnection(GetConnectionString());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to create database connection");
            throw;
        }
    }
}
