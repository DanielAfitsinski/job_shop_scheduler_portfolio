public class MenuController(MenuView view, IScenarioProvider scenarioProvider)
{
    private readonly record struct AlgorithmOption(AlgorithmId Id, string DisplayName);

    private static readonly KeyValuePair<AlgorithmCategory, string>[] CategoryChoices =
    [
        new(AlgorithmCategory.SimpleHeuristics, "Simple Heuristics"),
        new(AlgorithmCategory.LocalSearch, "Local search"),
        new(AlgorithmCategory.Evolutionary, "Evolutionary"),
        new(AlgorithmCategory.Benchmark, "Benchmark")
    ];

    private static readonly IReadOnlyDictionary<AlgorithmCategory, AlgorithmOption[]> AlgorithmChoicesByCategory =
        new Dictionary<AlgorithmCategory, AlgorithmOption[]>
        {
            [AlgorithmCategory.SimpleHeuristics] = [new(AlgorithmId.ShortestProcessingTime, "Shortest Processing Time")],
            [AlgorithmCategory.LocalSearch] = [
                new(AlgorithmId.HillClimbing, "Hill Climbing"),
                new(AlgorithmId.TabuSearch, "Tabu Search")],
            [AlgorithmCategory.Evolutionary] = [
                new(AlgorithmId.GeneticAlgorithm, "Genetic Algorithm"),
                new(AlgorithmId.MemeticHybrid, "Memetic Hybrid")],
            [AlgorithmCategory.Benchmark] = [new(AlgorithmId.GoogleOrTools, "Google OR-Tools")]
        };

    private static readonly IReadOnlyList<ISchedulingAlgorithm> Algorithms =
    [
        new ShortestProcessingTimeAlgorithm(),
        new HillClimbingAlgorithm()
    ];

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
                string[] categoryOptions = [.. CategoryChoices.Select(choice => choice.Value)];

                MenuSelectionResult selectedCategory = view.PromptSelection(
                    "Select Category",
                    categoryOptions,
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

                AlgorithmCategory selectedCategoryValue = CategoryChoices[selectedCategory.SelectedIndex].Key;
                string selectedCategoryName = CategoryChoices[selectedCategory.SelectedIndex].Value;
                AlgorithmOption[] algorithmOptions = GetAlgorithmOptions(selectedCategoryValue);
                string[] algorithmDisplayNames = [.. algorithmOptions.Select(option => option.DisplayName)];

                MenuSelectionResult selectedAlgorithmResult = view.PromptSelection(
                    $"Select Algorithm - {selectedCategoryName}",
                    algorithmDisplayNames,
                    "_Run",
                    "_Back",
                    MenuSelectionAction.Back,
                    60,
                    14,
                    0);
                if (selectedAlgorithmResult.Action == MenuSelectionAction.Back)
                {
                    continue;
                }

                if (selectedAlgorithmResult.Action != MenuSelectionAction.Confirmed)
                {
                    return;
                }

                AlgorithmOption selectedAlgorithm = algorithmOptions[selectedAlgorithmResult.SelectedIndex];

                MenuSelectionAction confirmation = view.PromptConfirmation(
                    "Confirm Selection",
                    $"Schedule: {GetScheduleName(schedule)}\nCategory: {selectedCategoryName}\nAlgorithm: {selectedAlgorithm.DisplayName}",
                    "_Confirm",
                    "_Back",
                    70,
                    10);

                if (confirmation == MenuSelectionAction.Back)
                {
                    continue;
                }

                RunSelectedAlgorithm(schedule, selectedCategoryName, selectedAlgorithm);
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

    private static AlgorithmOption[] GetAlgorithmOptions(AlgorithmCategory category)
    {
        return AlgorithmChoicesByCategory.TryGetValue(category, out AlgorithmOption[]? options)
            ? options
            : [];
    }

    private static void RunSelectedAlgorithm(
        Schedule schedule,
        string categoryName,
        AlgorithmOption selectedAlgorithm)
    {
        ISchedulingAlgorithm? algorithm = Algorithms.FirstOrDefault(candidate => candidate.Id == selectedAlgorithm.Id);

        if (algorithm is null)
        {
            MenuView.ShowInfo(
                "Algorithm Not Implemented",
                $"Schedule: {GetScheduleName(schedule)}\nCategory: {categoryName}\nAlgorithm: {selectedAlgorithm.DisplayName}\n\nThis algorithm path is not implemented yet.");
            return;
        }

        AlgorithmExecutionResult result = algorithm.Execute(schedule);
        if (result.IsError)
        {
            MenuView.ShowError(result.Title, result.Message);
            return;
        }

        MenuView.ShowInfo(result.Title, result.Message, result.DialogWidth, result.DialogHeight);
    }
}
