using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Serilog;

namespace AvaloniaApplication1.Service
{
    public class DatabaseManager
    {
        private readonly string _dbPath;

        public DatabaseManager(string? dbPath = null)
        {
            // Use a cross-platform path in the Data directory
            string dbFilePath = dbPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "users.db");
            _dbPath = $"Data Source={dbFilePath}";
            Log.Information("Initializing database with path: {DbPath}", _dbPath);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                // Ensure the Data directory exists
                string directory = Path.GetDirectoryName(_dbPath.Replace("Data Source=", ""))!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var connection = new SqliteConnection(_dbPath);
                connection.Open();

                // Create Users table
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();

                // Create Logs table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Logs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL,
                        LoginTime TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Settings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Key TEXT NOT NULL UNIQUE,
                        Value TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();

                // Insert default admin user for testing (remove in production)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword("123456");
                command.CommandText = @"
                    INSERT OR IGNORE INTO Users (Username, Password)
                    VALUES ('admin', $hashedPassword)";
                command.Parameters.AddWithValue("$hashedPassword", hashedPassword);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                throw new Exception($"Failed to initialize database: {ex.Message}");
            }
        }

        public void SaveRememberedCredentials(string username, string? encryptedPassword)
        {
            try
            {
                using var connection = new SqliteConnection(_dbPath);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT OR REPLACE INTO Settings (Key, Value)
            VALUES ('RememberedUsername', $username),
                   ('RememberedPassword', $password)";
                command.Parameters.AddWithValue("$username", username);
                command.Parameters.AddWithValue("$password", encryptedPassword ?? "");
                command.ExecuteNonQuery();
                Log.Information("Saved remembered credentials for {Username}", username);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Failed to save remembered credentials");
                throw new Exception($"Failed to save remembered credentials: {ex.Message}");
            }
        }

        public (string Username, string? EncryptedPassword)? LoadRememberedCredentials()
        {
            try
            {
                using var connection = new SqliteConnection(_dbPath);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT Value FROM Settings WHERE Key = 'RememberedUsername'
            UNION ALL
            SELECT Value FROM Settings WHERE Key = 'RememberedPassword'";
                using var reader = command.ExecuteReader();

                string? username = null;
                string? encryptedPassword = null;
                int index = 0;
                while (reader.Read())
                {
                    if (index == 0) username = reader.GetString(0);
                    else if (index == 1) encryptedPassword = reader.GetString(0);
                    index++;
                }

                if (username != null)
                {
                    return (username, encryptedPassword);
                }

                return null;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Failed to load remembered credentials");
                return null;
            }
        }

        public void ClearRememberedCredentials()
        {
            try
            {
                using var connection = new SqliteConnection(_dbPath);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            DELETE FROM Settings WHERE Key IN ('RememberedUsername', 'RememberedPassword')";
                command.ExecuteNonQuery();
                Log.Information("Cleared remembered credentials");
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Failed to clear remembered credentials");
                throw new Exception($"Failed to clear remembered credentials: {ex.Message}");
            }
        }

        public bool AuthenticateUser(string username, string password)
        {
            try
            {
                using var connection = new SqliteConnection(_dbPath);
                connection.Open();
                Log.Information("Authenticating user: {Username}", username);

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Password FROM Users WHERE Username = $username";
                command.Parameters.AddWithValue("$username", username);

                string? storedHash = command.ExecuteScalar() as string;
                bool isValid = storedHash != null && BCrypt.Net.BCrypt.Verify(password, storedHash);
                Log.Information("Authentication result for {Username}: {Result}", username, isValid);
                return isValid;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Authentication failed for {Username}", username);
                throw new Exception($"Authentication error: {ex.Message}");
            }
        }

        public void LogLoginActivity(string username)
        {
            try
            {
                using var connection = new SqliteConnection(_dbPath);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Logs (Username, LoginTime)
                    VALUES ($username, $loginTime)";
                command.Parameters.AddWithValue("$username", username);
                command.Parameters.AddWithValue("$loginTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                throw new Exception($"Failed to log login activity: {ex.Message}");
            }
        }
    }
}