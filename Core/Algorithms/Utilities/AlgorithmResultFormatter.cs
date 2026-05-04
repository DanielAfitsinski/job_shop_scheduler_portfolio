namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Builds the standard summary text shown for algorithm execution results
public static class AlgorithmResultFormatter
{
    public static string BuildStandardMessage(
        Schedule schedule,
        string algorithmName,
        int taskCount,
        int makespan)
    {
        return
            $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\n" +
            $"Algorithm: {algorithmName}\n" +
            "Objective: Minimise makespan\n" +
            $"Task count: {taskCount}\n" +
            $"Final makespan: {makespan}";
    }
}