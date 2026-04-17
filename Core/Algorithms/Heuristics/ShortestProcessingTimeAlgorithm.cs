namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Baseline heuristic that orders tasks by processing time
public class ShortestProcessingTimeAlgorithm : SimpleHeuristicAlgorithm
{
    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.ShortestProcessingTime;
    // Algorithm display name
    public override string DisplayName => "Shortest Processing Time";

    // Orders tasks by processing time, then job, then operation
    public override IReadOnlyList<JSPTask> BuildSequence(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        // Use a deterministic sort to build the heuristic sequence
        return [.. schedule.tasks
            .OrderBy(task => task.ProcessingTime)
            .ThenBy(task => task.JobId)
            .ThenBy(task => task.Operation)];
    }
}