public class ShortestProcessingTimeAlgorithm : ISchedulingAlgorithm
{
    public AlgorithmCategory Category => AlgorithmCategory.SimpleHeuristics;
    public AlgorithmId Id => AlgorithmId.ShortestProcessingTime;
    public string DisplayName => "Shortest Processing Time";

    public AlgorithmExecutionResult Execute(Schedule schedule)
    {
        IReadOnlyList<JSPTask> orderedTasks = BuildSequence(schedule);
        if (orderedTasks.Count == 0)
        {
            return new AlgorithmExecutionResult("No Tasks", "The selected schedule has no tasks.", isError: true);
        }

        int totalProcessingTime = CalculateTotalProcessingTime(orderedTasks);

        string message =
            $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\n" +
            $"Algorithm: {DisplayName}\n" +
            $"Task count: {orderedTasks.Count}\n" +
            $"Total processing time: {totalProcessingTime}";

        return new AlgorithmExecutionResult(
            "Shortest Processing Time Result",
            message);
    }

    public static IReadOnlyList<JSPTask> BuildSequence(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        return [.. schedule.tasks
            .OrderBy(task => task.ProcessingTime)
            .ThenBy(task => task.JobId)
            .ThenBy(task => task.Operation)];
    }

    public static int CalculateTotalProcessingTime(IEnumerable<JSPTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        return tasks.Sum(task => task.ProcessingTime);
    }

}