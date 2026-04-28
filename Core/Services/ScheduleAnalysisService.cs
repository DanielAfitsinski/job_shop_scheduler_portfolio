namespace Job_Shop_Scheduler_Portfolio.Core.Services;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Analyses computed schedules and generates statistics about job completion and resource utilisation
public static class ScheduleAnalysisService
{
    // Analyses an ordered schedule and computes statistics for reporting
    public static AnalysisResult Analyse(
        string? scheduleName,
        string? algorithmName,
        IReadOnlyList<JSPTask> orderedTasks,
        int makespan)
    {
        ArgumentNullException.ThrowIfNull(orderedTasks);

        int totalJobs = orderedTasks.Select(t => t.JobId).Distinct().Count();
        double avgTimePerJob = CalculateAverageTimePerJob(orderedTasks, totalJobs);
        var subdivisionStats = BuildSubdivisionStatistics(orderedTasks);
        var scheduledTasks = CalculateScheduleTiming(orderedTasks);

        return new AnalysisResult
        {
            ScheduleName = scheduleName,
            AlgorithmName = algorithmName,
            TotalMakespan = makespan,
            TotalJobs = totalJobs,
            TotalOperations = orderedTasks.Count,
            AverageTimePerJob = avgTimePerJob,
            SubdivisionStats = subdivisionStats,
            ScheduledTasks = scheduledTasks
        };
    }

    // Calculates the average time per job in an ordered task list
    private static double CalculateAverageTimePerJob(IReadOnlyList<JSPTask> tasks, int totalJobs)
    {
        if (totalJobs == 0)
        {
            return 0.0;
        }
        return (double)tasks.Sum(t => t.ProcessingTime) / totalJobs;
    }

    // Groups tasks by subdivision and aggregates operation statistics
    private static Dictionary<string, SubdivisionStatistics> BuildSubdivisionStatistics(IReadOnlyList<JSPTask> tasks)
    {
        var subdivisionGroups = tasks
            .GroupBy(t => t.SubDivision ?? "Unknown")
            .OrderBy(g => g.Key);

        var stats = new Dictionary<string, SubdivisionStatistics>();
        foreach (var group in subdivisionGroups)
        {
            var subdivisionStat = new SubdivisionStatistics
            {
                OperationCount = group.Count(),
                TotalProcessingTime = group.Sum(t => t.ProcessingTime)
            };

            foreach (var task in group)
            {
                subdivisionStat.Operations.Add(new OperationDetail
                {
                    JobId = task.JobId,
                    OperationNumber = task.Operation,
                    ProcessingTime = task.ProcessingTime
                });
            }

            stats[group.Key] = subdivisionStat;
        }
        return stats;
    }

    // Calculates start and end times for each task
    private static List<ScheduledTaskDetail> CalculateScheduleTiming(IReadOnlyList<JSPTask> tasks)
    {
        var scheduledTasks = new List<ScheduledTaskDetail>();
        string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        // Track when each subdivision/machine becomes available
        var subdivisionAvailability = new Dictionary<string, int>();
        // Track when each job's last task completes
        var jobAvailability = new Dictionary<int, int>();

        foreach (var task in tasks)
        {
            int processingHours = task.ProcessingTime;
            string subdivision = task.SubDivision ?? "Unknown";

            // Get the earliest time this machine is available
            int machineAvailableAt = subdivisionAvailability.TryGetValue(subdivision, out var machineTime) ? machineTime : 0;

            // Get the earliest time this job can run another task
            int jobAvailableAt = jobAvailability.TryGetValue(task.JobId, out var jobTime) ? jobTime : 0;

            // Task can only start when both machine and job are available
            int startCumulativeHours = Math.Max(machineAvailableAt, jobAvailableAt);
            int endCumulativeHours = startCumulativeHours + processingHours;

            // Calculate start time (day and hour)
            var (startDay, startHour) = ConvertHoursToDayAndTime(startCumulativeHours, daysOfWeek);
            // Calculate end time (day and hour)
            var (endDay, endHour) = ConvertHoursToDayAndTime(endCumulativeHours, daysOfWeek);

            scheduledTasks.Add(new ScheduledTaskDetail
            {
                JobId = task.JobId,
                Operation = task.Operation,
                SubDivision = subdivision,
                ProcessingTimeHours = processingHours,
                StartDay = startDay,
                StartHour = startHour,
                EndDay = endDay,
                EndHour = endHour,
                CumulativeStartHour = startCumulativeHours,
                CumulativeEndHour = endCumulativeHours
            });

            // Update availability tracking
            subdivisionAvailability[subdivision] = endCumulativeHours;
            jobAvailability[task.JobId] = endCumulativeHours;
        }

        return scheduledTasks;
    }

    // Converts cumulative hours from schedule start into a day of week and hour (0-23)
    // Days cycle weekly, starting with Monday
    private static (string day, int hour) ConvertHoursToDayAndTime(int cumulativeHours, string[] daysOfWeek)
    {
        const int hoursPerDay = 24;
        int dayIndex = (cumulativeHours / hoursPerDay) % daysOfWeek.Length;
        int hourOfDay = cumulativeHours % hoursPerDay;
        
        return (daysOfWeek[dayIndex], hourOfDay);
    }
}
