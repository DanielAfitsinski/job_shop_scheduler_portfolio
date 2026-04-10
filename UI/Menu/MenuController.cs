public class MenuController(MenuView view, IScenarioProvider scenarioProvider)
{
    private static readonly string[] CategoryOptions = ["Simple Heuristics", "Local search", "Evolutionary", "Benchmark"];

    private readonly MenuView view = view;
    private readonly IScenarioProvider scenarioProvider = scenarioProvider;

    public void Run()
    {
        MenuView.Initialise();
        MenuView.ShowMainScreen(OnRunSelected, OnExitSelected);
        MenuView.RunMainLoop();
        MenuView.Shutdown();
    }

    private void OnRunSelected()
    {
        while (true)
        {
            IReadOnlyList<Schedule> schedules = scenarioProvider.GetSchedules();
            if (schedules.Count == 0)
            {
                MenuView.ShowError("No Schedules", "No schedules are loaded.");
                return;
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
                return;
            }

            Schedule schedule = schedules[selectedSchedule.SelectedIndex];

            while (true)
            {
                MenuSelectionResult selectedCategory = view.PromptSelection(
                    "Select Category",
                    CategoryOptions,
                    "_Next",
                    "_Back",
                    MenuSelectionAction.Back,
                    60,
                    14,
                    0);
                if (selectedCategory.Action == MenuSelectionAction.Back)
                {
                    break;
                }

                if (selectedCategory.Action != MenuSelectionAction.Confirmed)
                {
                    return;
                }

                AlgorithmCategory selectedCategoryValue = GetCategory(selectedCategory.SelectedIndex);
                string[] algorithmOptions = GetAlgorithmOptions(selectedCategoryValue);

                MenuSelectionResult selectedAlgorithm = view.PromptSelection(
                    $"Select Algorithm - {CategoryOptions[selectedCategory.SelectedIndex]}",
                    algorithmOptions,
                    "_Run",
                    "_Back",
                    MenuSelectionAction.Back,
                    60,
                    14,
                    0);
                if (selectedAlgorithm.Action == MenuSelectionAction.Back)
                {
                    continue;
                }

                if (selectedAlgorithm.Action != MenuSelectionAction.Confirmed)
                {
                    return;
                }

                string selectedAlgorithmName = algorithmOptions[selectedAlgorithm.SelectedIndex];

                MenuSelectionAction confirmation = view.PromptConfirmation(
                    "Confirm Selection",
                    $"Schedule: {GetScheduleName(schedule)}\nCategory: {CategoryOptions[selectedCategory.SelectedIndex]}\nAlgorithm: {selectedAlgorithmName}",
                    "_Confirm",
                    "_Back",
                    70,
                    10);

                if (confirmation == MenuSelectionAction.Back)
                {
                    continue;
                }

                MenuView.ShowInfo(
                    "Selection Complete",
                    $"Schedule: {GetScheduleName(schedule)}\nCategory: {CategoryOptions[selectedCategory.SelectedIndex]}\nAlgorithm: {selectedAlgorithmName}"
                );
                return;
            }
        }
    }

    private void OnExitSelected()
    {
        MenuView.RequestStop();
    }

    private static string GetScheduleName(Schedule schedule)
    {
        return schedule.ScheduleName ?? "Unnamed schedule";
    }

    private static AlgorithmCategory GetCategory(int selectedCategoryIndex)
    {
        return selectedCategoryIndex switch
        {
            0 => AlgorithmCategory.SimpleHeuristics,
            1 => AlgorithmCategory.LocalSearch,
            2 => AlgorithmCategory.Evolutionary,
            3 => AlgorithmCategory.Benchmark,
            _ => throw new ArgumentOutOfRangeException(nameof(selectedCategoryIndex))
        };
    }

    private static string[] GetAlgorithmOptions(AlgorithmCategory category)
    {
        return category switch
        {
            AlgorithmCategory.SimpleHeuristics => ["Shortest Processing Time"],
            AlgorithmCategory.LocalSearch => ["Hill Climbing", "Tabu Search"],
            AlgorithmCategory.Evolutionary => ["Genetic Algorithm", "Memetic Hybrid"],
            AlgorithmCategory.Benchmark => ["Google OR-Tools"],
            _ => ["No algorithms available"]
        };
    }
}
