namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Baseline heuristic that orders tasks in random order
public class RandomAlgorithm : SimpleHeuristicAlgorithm
{
    private readonly Random _random;

    public RandomAlgorithm()
    {
        _random = new Random();
    }

    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.RandomHeuristic;
    // Algorithm display name
    public override string DisplayName => "Random Heuristic";

    // Orders tasks in random order
    public override IReadOnlyList<JSPTask> BuildSequence(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        List<JSPTask> tasks = [.. schedule.tasks];
        // Shuffle the tasks randomly
        for (int i = tasks.Count - 1; i > 0; i--)
        {
            int randomIndex = _random.Next(i + 1);
            (tasks[i], tasks[randomIndex]) = (tasks[randomIndex], tasks[i]);
        }

        return tasks;
    }
}
