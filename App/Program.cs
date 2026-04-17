
using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Implementations;
using Job_Shop_Scheduler_Portfolio.UI.Menu.Controllers;

namespace Job_Shop_Scheduler_Portfolio
{
    // Application entrypoint for the console scheduler
    class Program
    {
        // Loads schedules and then hands control to the menu loop
        static void Main(string[] args)
        {
            // Preload the scenario files before showing the UI
            try
            {
                FileManager.PreloadJobFiles();
            }
            catch (Exception ex)
            {
                // Stop startup if schedule loading fails
                Console.WriteLine($"Failed to load schedules at startup: {ex.Message}");
                return;
            }

            // Start the interactive menu system
            Menu.Run();
        }

    }
}