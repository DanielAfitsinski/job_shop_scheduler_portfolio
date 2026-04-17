namespace Job_Shop_Scheduler_Portfolio.UI.Menu.Controllers;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Abstractions;
using Job_Shop_Scheduler_Portfolio.UI.Menu.Views;
using Job_Shop_Scheduler_Portfolio.UI.Menu.Models;

// Coordinates menu navigation and dispatches selected algorithms
public class MenuController(MenuView view, IScenarioProvider scenarioProvider, AlgorithmExecutionService algorithmExecutionService)
{
    // Menu option record used for algorithm selection
    private readonly record struct AlgorithmOption(AlgorithmId Id, string DisplayName);

    // The algorithms shown directly to the user
    private static readonly AlgorithmOption[] AlgorithmChoices =
    [
        new(AlgorithmId.TabuSearch, "Tabu Search"),
        new(AlgorithmId.GeneticAlgorithm, "Genetic Algorithm"),
        new(AlgorithmId.MemeticHybrid, "Memetic Hybrid")
    ];

    // The injected view layer
    private readonly MenuView view = view;
    // The injected schedule provider
    private readonly IScenarioProvider scenarioProvider = scenarioProvider;
    // The service that runs the chosen algorithm
    private readonly AlgorithmExecutionService algorithmExecutionService = algorithmExecutionService;

    // Shows the UI and keeps it running until the user exits
    public void Run()
    {
        MenuView.Initialise();
        MenuView.ShowMainScreen(OnRunSelected, OnExitSelected);
        MenuView.RunMainLoop();
        MenuView.Shutdown();
    }

    // Handles the run workflow from schedule selection to execution
    private void OnRunSelected()
    {
        while (true)
        {
            Schedule? schedule = SelectSchedule();
            if (schedule is null)
            {
                return;
            }

            bool shouldReturnToScheduleSelection = RunAlgorithmWorkflow(schedule);
            if (!shouldReturnToScheduleSelection)
            {
                return;
            }
        }
    }

    // Prompts the user to select a schedule, returns null if cancelled or no schedules available
    private Schedule? SelectSchedule()
    {
        IReadOnlyList<Schedule> schedules = scenarioProvider.GetSchedules();
        if (schedules.Count == 0)
        {
            MenuView.ShowError("No Schedules", "No schedules are loaded.");
            return null;
        }

        string[] scheduleNames = [.. schedules.Select(GetScheduleName)];
        MenuSelectionResult selectedSchedule = view.PromptSelection(
            "Select Schedule",
            scheduleNames,
            "_Next",
            "_Cancel",
            MenuSelectionAction.Cancel,
            70,
            20);

        if (selectedSchedule.Action != MenuSelectionAction.Confirmed)
        {
            return null;
        }

        return schedules[selectedSchedule.SelectedIndex];
    }

    // Runs the algorithm selection and execution workflow for a given schedule
    // Returns true if the user wants to return to schedule selection, false if exiting entirely
    private bool RunAlgorithmWorkflow(Schedule schedule)
    {
        while (true)
        {
            AlgorithmOption? selectedAlgorithm = SelectAlgorithm();
            if (selectedAlgorithm is null)
            {
                return false;
            }

            if (selectedAlgorithm.Value.Id == default)
            {
                // User clicked back - return to schedule selection
                return true;
            }

            if (!ConfirmAndExecute(schedule, selectedAlgorithm.Value))
            {
                // User clicked back - return to algorithm selection
                continue;
            }

            // Successful execution - exit the entire workflow
            return false;
        }
    }

    // Prompts the user to select an algorithm
    // Returns null if user cancels, or AlgorithmOption with Id = default if user goes back
    private AlgorithmOption? SelectAlgorithm()
    {
        string[] algorithmDisplayNames = [.. AlgorithmChoices.Select(option => option.DisplayName)];
        MenuSelectionResult selectedAlgorithmResult = view.PromptSelection(
            "Select Algorithm",
            algorithmDisplayNames,
            "_Run",
            "_Back",
            MenuSelectionAction.Back,
            60,
            14,
            0);

        return selectedAlgorithmResult.Action switch
        {
            MenuSelectionAction.Back => new AlgorithmOption(default, ""),
            MenuSelectionAction.Confirmed => AlgorithmChoices[selectedAlgorithmResult.SelectedIndex],
            _ => null
        };
    }

    // Shows confirmation and executes the algorithm, returns true if execution completed successfully
    private bool ConfirmAndExecute(Schedule schedule, AlgorithmOption algorithm)
    {
        MenuSelectionAction confirmation = view.PromptConfirmation(
            "Confirm Selection",
            $"Schedule: {GetScheduleName(schedule)}\nAlgorithm: {algorithm.DisplayName}",
            "_Confirm",
            "_Back",
            70,
            10);

        if (confirmation == MenuSelectionAction.Back)
        {
            return false;
        }

        AlgorithmExecutionResult result = algorithmExecutionService.Execute(schedule, algorithm.Id);
        if (result.IsError)
        {
            MenuView.ShowError(result.Title, result.Message);
            return false;
        }

        MenuView.ShowInfo(result.Title, result.Message, result.DialogWidth, result.DialogHeight);
        return true;
    }

    // Stops the menu loop when the user chooses to exit
    private void OnExitSelected()
    {
        MenuView.RequestStop();
    }

    // Returns a schedule name for the UI
    private static string GetScheduleName(Schedule schedule)
    {
        return schedule.ScheduleName ?? "Unnamed schedule";
    }

}
