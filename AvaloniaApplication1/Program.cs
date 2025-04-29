using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using SQLitePCL; // For Batteries.Init()
using Serilog; // Optional: For logging
using System;
using System.IO;

namespace AvaloniaApplication1
{
    sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Optional: Initialize Serilog for logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "app.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
            try
            {
                // Initialize SQLitePCL provider
                Batteries.Init();

                // Start the Avalonia application
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw; // Rethrow to ensure the application fails visibly during development
            }
            finally
            {
                Log.CloseAndFlush(); // Ensure all logs are written
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}