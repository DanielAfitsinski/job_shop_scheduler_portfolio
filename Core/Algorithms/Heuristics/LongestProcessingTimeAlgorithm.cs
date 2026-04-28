namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Baseline heuristic that orders tasks by descending processing time
public class LongestProcessingTimeAlgorithm : SimpleHeuristicAlgorithm
{
    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.LongestProcessingTime;
    // Algorithm display name
    public override string DisplayName => "Longest Processing Time";

    // Orders tasks by descending processing time, then job, then operation
    public override IReadOnlyList<JSPTask> BuildSequence(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        
        return [.. schedule.tasks
            .OrderByDescending(task => task.ProcessingTime)
            .ThenBy(task => task.JobId)
            .ThenBy(task => task.Operation)];
    }
}
