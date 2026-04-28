namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Base class for simple construction heuristics that share common execution patterns
public abstract class SimpleHeuristicAlgorithm : ISchedulingAlgorithm
{
    // All simple heuristics belong to this category
    public AlgorithmCategory Category => AlgorithmCategory.SimpleHeuristics;

    // Subclasses define their specific algorithm identifier
    public abstract AlgorithmId Id { get; }

    // Subclasses define their display name
    public abstract string DisplayName { get; }

    // Common execute method for all simple heuristics
    public AlgorithmExecutionResult Execute(Schedule schedule)
    {
        IReadOnlyList<JSPTask> orderedTasks = BuildSequence(schedule);
        if (orderedTasks.Count == 0)
        {
            return new AlgorithmExecutionResult("No Tasks", "The selected schedule has no tasks.", isError: true);
        }

        int totalProcessingTime = ScheduleEvaluation.CalculateTotalProcessingTime(orderedTasks);
        int makespan = ScheduleEvaluation.CalculateMakespan(schedule, orderedTasks);

        return BuildResultMessage(schedule, orderedTasks, totalProcessingTime, makespan);
    }

    // Subclasses implement their specific sequence building strategy
    public abstract IReadOnlyList<JSPTask> BuildSequence(Schedule schedule);

    // Common result message formatting for all simple heuristics
    protected AlgorithmExecutionResult BuildResultMessage(Schedule schedule, IReadOnlyList<JSPTask> orderedTasks, int totalProcessingTime, int makespan)
    {
        string message =
            $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\n" +
            $"Algorithm: {DisplayName}\n" +
            $"Task count: {orderedTasks.Count}\n" +
            $"Total processing time: {totalProcessingTime}\n" +
            $"Makespan: {makespan}";

        return new AlgorithmExecutionResult(
            $"{DisplayName} Result",
            message,
            computedSchedule: orderedTasks,
            makespan: makespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }
}
