namespace Job_Shop_Scheduler_Portfolio.UI.Menu.Controllers;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services;
using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Implementations;
using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Abstractions;
using Job_Shop_Scheduler_Portfolio.UI.Menu.Views;

// Thin entry point for constructing and running the menu workflow
public class Menu
{
    // Creates the UI and scenario provider, then starts the controller
    public static void Run()
    {
        // Load schedules through the scenario provider
        IScenarioProvider scenarioProvider = new FileManagerScenarioProvider();
        // Create the algorithm execution service used by the controller
        AlgorithmExecutionService algorithmExecutionService = new();
        // Create the menu view used for TerminalGui interactions
        MenuView view = new();
        // Connect the controller to the view and the scenario provider
        MenuController controller = new(view, scenarioProvider, algorithmExecutionService);
        // Start the UI loop
        controller.Run();
    }
}