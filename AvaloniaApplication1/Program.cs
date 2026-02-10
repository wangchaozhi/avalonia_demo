using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using SQLitePCL; // For Batteries.Init()
using Serilog; // Optional: For logging
using System;
using System.IO;
using System.Threading.Tasks;
using AvaloniaApplication1.Service;

namespace AvaloniaApplication1
{
    sealed class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            // Single-instance guard (per user session).
            var instanceKey = $"{typeof(Program).Assembly.GetName().Name}.SingleInstance.{Environment.UserName}";
            if (!SingleInstanceManager.InitializeAsFirstInstance(instanceKey))
            {
                await SingleInstanceManager.SignalFirstInstanceAsync(instanceKey);
                return;
            }

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
                SingleInstanceManager.Shutdown();
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