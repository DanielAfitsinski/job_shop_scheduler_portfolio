namespace Job_Shop_Scheduler_Portfolio.UI.Menu.Controllers;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services.Factories;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Factories;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Services;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Job_Shop_Scheduler_Portfolio.Core.Services;
using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Abstractions;
using Job_Shop_Scheduler_Portfolio.UI.Menu.Views;
using Job_Shop_Scheduler_Portfolio.UI.Menu.Models;

// Coordinates menu navigation and dispatches selected algorithms
public class MenuController(MenuView view, IScenarioProvider scenarioProvider, AlgorithmExecutionService algorithmExecutionService)
{
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
        MenuSelectionResult selectedSchedule = MenuView.PromptSelection(
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
            IAlgorithmDescriptor? selectedAlgorithm = SelectAlgorithm();
            if (selectedAlgorithm is null)
            {
                return false;
            }

            if (!ConfirmAndExecute(schedule, selectedAlgorithm))
            {
                // User clicked back - return to algorithm selection
                continue;
            }

            // Successful execution - exit the entire workflow
            return false;
        }
    }

    // Prompts the user to select an algorithm
    // Returns null if user cancels or goes back
    private static IAlgorithmDescriptor? SelectAlgorithm()
    {
        var availableAlgorithms = AlgorithmFactory.GetAvailableAlgorithms();
        string[] algorithmDisplayNames = [.. availableAlgorithms.Select(a => a.DisplayName)];
        MenuSelectionResult selectedAlgorithmResult = MenuView.PromptSelection(
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
            MenuSelectionAction.Back => null,
            MenuSelectionAction.Confirmed => availableAlgorithms[selectedAlgorithmResult.SelectedIndex],
            _ => null
        };
    }

    // Shows confirmation and executes the algorithm with optional parameter and operator configuration
    private bool ConfirmAndExecute(Schedule schedule, IAlgorithmDescriptor algorithm)
    {
        MenuSelectionAction confirmation = MenuView.PromptConfirmation(
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

        // Ask if user wants to customise parameters
        // If user cancels during parameter configuration, go back to algorithm selection
        if (!TryGetCustomParameters(algorithm.Id, out IAlgorithmParameters? customParameters))
        {
            return false; // User cancelled - go back to algorithm selection
        }

        // Ask if user wants to customise genetic operators (for genetic-based algorithms)
        (ICrossoverOperator? crossoverOperator, IMutationOperator? mutationOperator) operators = 
            PromptForOperatorConfiguration(algorithm.Id);

        AlgorithmExecutionResult result = algorithmExecutionService.Execute(
            schedule, 
            algorithm.Id, 
            customParameters,
            operators.crossoverOperator,
            operators.mutationOperator);
        
        if (result.IsError)
        {
            MenuView.ShowError(result.Title, result.Message);
            return false;
        }

        MenuView.ShowInfo(result.Title, result.Message, result.DialogWidth, result.DialogHeight);
        
        // Display results summary with export option if computation was successful
        if (result.ComputedSchedule != null && result.ComputedSchedule.Count > 0)
        {
            DisplayAndExportResults(result);
        }
        
        return true;
    }

    // Attempts to configure custom algorithm parameters
    // Returns true if configuration succeeded or user chose defaults
    // Returns false if user cancelled (should go back to algorithm selection)
    // Outputs the parameters via the out parameter (null means use defaults)
    private bool TryGetCustomParameters(AlgorithmId algorithmId, out IAlgorithmParameters? parameters)
    {
        parameters = null;

        MenuSelectionAction configChoice = MenuView.PromptConfirmation(
            "Parameter Configuration",
            "Use custom parameters?",
            "_Yes",
            "_No (use default)",
            60,
            8);

        if (configChoice != MenuSelectionAction.Confirmed)
        {
            // User chose not to customise - use defaults and continue
            return true;
        }

        // User wants to customise - check if this algorithm type supports custom parameters
        bool isConfigurableAlgorithm = algorithmId switch
        {
            AlgorithmId.TabuSearch or
            AlgorithmId.HillClimbing or
            AlgorithmId.GeneticAlgorithm or
            AlgorithmId.MemeticHybrid => true,
            _ => false
        };

        if (!isConfigurableAlgorithm)
        {
            // Algorithm doesn't support custom parameters, use defaults
            return true;
        }

        // Configure based on algorithm type
        parameters = algorithmId switch
        {
            AlgorithmId.TabuSearch => GetCustomTabuSearchParameters(),
            AlgorithmId.HillClimbing => GetCustomLocalSearchParameters(),
            AlgorithmId.GeneticAlgorithm => GetCustomEvolutionaryParameters(),
            AlgorithmId.MemeticHybrid => GetCustomEvolutionaryParameters(),
            _ => null
        };

        // If parameters is null at this point, it means user cancelled during configuration
        if (parameters == null)
        {
            return false; // User cancelled - go back to algorithm selection
        }

        return true; // Configuration succeeded with custom parameters
    }

    // Prompts user to optionally configure genetic operators (crossover and mutation)
    private (ICrossoverOperator?, IMutationOperator?) PromptForOperatorConfiguration(AlgorithmId algorithmId)
    {
        // Only prompt for operators if the algorithm uses them (GeneticAlgorithm and MemeticHybrid)
        if (algorithmId != AlgorithmId.GeneticAlgorithm && algorithmId != AlgorithmId.MemeticHybrid)
        {
            return (null, null);
        }

        MenuSelectionAction operatorChoice = MenuView.PromptConfirmation(
            "Operator Configuration",
            "Customise genetic operators?",
            "_Yes",
            "_No (use default)",
            60,
            8);

        if (operatorChoice != MenuSelectionAction.Confirmed)
        {
            return (null, null); // Use default operators
        }

        // Prompt for crossover operator
        ICrossoverOperator? crossoverOperator = PromptForCrossoverOperator();
        
        // Prompt for mutation operator
        IMutationOperator? mutationOperator = PromptForMutationOperator();

        return (crossoverOperator, mutationOperator);
    }

    // Prompts user to select a crossover operator
    private static ICrossoverOperator? PromptForCrossoverOperator()
    {
        var availableOperators = CrossoverOperatorFactory.GetAvailableOperators();
        string[] operatorNames = [.. availableOperators.Select(op => op.DisplayName)];

        MenuSelectionResult result = MenuView.PromptSelection(
            "Crossover Operator",
            operatorNames,
            "_Select",
            "_Skip",
            MenuSelectionAction.Cancel,
            50,
            10,
            0);

        if (result.Action != MenuSelectionAction.Confirmed)
        {
            return null; // Use default
        }

        return CrossoverOperatorFactory.CreateByIndex(result.SelectedIndex);
    }

    // Prompts user to select a mutation operator
    private static IMutationOperator? PromptForMutationOperator()
    {
        var availableOperators = MutationOperatorFactory.GetAvailableOperators();
        string[] operatorNames = [.. availableOperators.Select(op => op.DisplayName)];

        MenuSelectionResult result = MenuView.PromptSelection(
            "Mutation Operator",
            operatorNames,
            "_Select",
            "_Skip",
            MenuSelectionAction.Cancel,
            50,
            10,
            0);

        if (result.Action != MenuSelectionAction.Confirmed)
        {
            return null; // Use default
        }

        return MutationOperatorFactory.CreateByIndex(result.SelectedIndex);
    }

    // Prompts user for custom Tabu Search parameters
    private static IAlgorithmParameters? GetCustomTabuSearchParameters()
    {
        int? maxIterations = MenuView.PromptIntInput(
            "Max Iterations",
            "Enter maximum iterations (500 default):",
            500,
            1,
            1000000);

        if (maxIterations == null)
            return null; // User cancelled

        int? multiStartSeeds = MenuView.PromptIntInput(
            "Multi-Start Seeds",
            "Enter number of multi-start seeds (5 default):",
            5,
            1,
            100);

        if (multiStartSeeds == null)
            return null; // User cancelled

        int? tabuTenure = MenuView.PromptIntInput(
            "Tabu Tenure",
            "Enter tabu tenure value (7 default):",
            7,
            1,
            maxIterations.Value / 2);

        if (tabuTenure == null)
            return null; // User cancelled

        // Early termination: stop if no improvement found for N iterations (useful for large problems)
        int? maxIterationsWithoutImprovement = MenuView.PromptIntInput(
            "Early Termination",
            "Max iterations without improvement before stopping (0 = disabled, recommended: 50-100 for large problems):",
            50,
            0,
            10000);

        if (maxIterationsWithoutImprovement == null)
            return null; // User cancelled

        return new TabuSearchParameters 
        { 
            ConfigurationName = "Custom",
            MaxIterations = maxIterations.Value,
            MultiStartSeeds = multiStartSeeds.Value,
            TabuTenure = tabuTenure.Value,
            MaxIterationsWithoutImprovement = maxIterationsWithoutImprovement.Value
        };
    }

    // Prompts user for custom Local Search parameters
    private static IAlgorithmParameters? GetCustomLocalSearchParameters()
    {
        int? maxIterations = MenuView.PromptIntInput(
            "Max Iterations",
            "Enter maximum iterations (1000 default):",
            1000,
            1,
            1000000);

        if (maxIterations == null)
            return null; // User cancelled

        int? multiStartSeeds = MenuView.PromptIntInput(
            "Multi-Start Seeds",
            "Enter number of multi-start seeds (5 default):",
            5,
            1,
            100);

        if (multiStartSeeds == null)
            return null; // User cancelled

        return new LocalSearchParameters 
        { 
            ConfigurationName = "Custom",
            MaxIterations = maxIterations.Value,
            MultiStartSeeds = multiStartSeeds.Value
        };
    }

    // Prompts user for custom Evolutionary Algorithm parameters
    private static IAlgorithmParameters? GetCustomEvolutionaryParameters()
    {
        int? populationSize = MenuView.PromptIntInput(
            "Population Size",
            "Enter population size (30 default):",
            30,
            10,
            200);

        if (populationSize == null)
            return null; // User cancelled

        int? generations = MenuView.PromptIntInput(
            "Generations",
            "Enter number of generations (80 default):",
            80,
            10,
            500);

        if (generations == null)
            return null; // User cancelled

        double? mutationRate = MenuView.PromptDoubleInput(
            "Mutation Rate",
            "Enter mutation rate as decimal 0.0-1.0 (0.20 default):",
            0.20,
            0.0,
            1.0);

        if (mutationRate == null)
            return null; // User cancelled

        int? eliteCount = MenuView.PromptIntInput(
            "Elite Count",
            "Enter elite count (2 default):",
            2,
            1,
            populationSize.Value / 2 - 1);

        if (eliteCount == null)
            return null; // User cancelled

        int? tournamentSize = MenuView.PromptIntInput(
            "Tournament Size",
            "Enter tournament size (3 default):",
            3,
            2,
            populationSize.Value / 2);

        if (tournamentSize == null)
            return null; // User cancelled

        return new EvolutionaryParameters 
        { 
            ConfigurationName = "Custom",
            PopulationSize = populationSize.Value,
            Generations = generations.Value,
            MutationRate = mutationRate.Value,
            EliteCount = eliteCount.Value,
            TournamentSize = tournamentSize.Value
        };
    }

    // Displays results summary with integrated export button
    private void DisplayAndExportResults(AlgorithmExecutionResult result)
    {
        try
        {
            var analysis = ScheduleAnalysisService.Analyse(
                result.ScheduleName,
                result.AlgorithmName,
                result.ComputedSchedule!,
                result.Makespan);

            string summaryTable = TableFormattingService.FormatAsSummary(analysis);
            
            // Show summary with Export/Skip buttons
            MenuSelectionAction exportChoice = MenuView.PromptConfirmation(
                "Results Summary",
                summaryTable,
                "_Export",
                "_Done",
                85,
                35);

            // Handle export if user clicked Export button
            if (exportChoice == MenuSelectionAction.Confirmed)
            {
                ExportResults(analysis, result);
            }
        }
        catch (Exception ex)
        {
            MenuView.ShowError("Display Error", $"Could not display results: {ex.Message}");
        }
    }

    // Exports analysis results to CSV file
    private static void ExportResults(AnalysisResult analysis, AlgorithmExecutionResult result)
    {
        try
        {
            string fileName = CsvExportService.GenerateFileName(result.ScheduleName, result.AlgorithmName);
            string outputDir = Path.Combine(AppContext.BaseDirectory, "Results");
            string filePath = Path.Combine(outputDir, fileName);

            CsvExportService.ExportToCsv(analysis, filePath);

            MenuView.ShowInfo(
                "Export Successful",
                $"Results exported to:\\n{filePath}",
                70,
                10);
        }
        catch (Exception ex)
        {
            MenuView.ShowError("Export Failed", $"Failed to export results:\\n{ex.Message}");
        }
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
