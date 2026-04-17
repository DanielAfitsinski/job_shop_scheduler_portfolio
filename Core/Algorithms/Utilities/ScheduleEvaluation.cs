namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Shared evaluation helpers for local-search and evolutionary algorithms
public static class ScheduleEvaluation
{
    // Computes the makespan of a task sequence while respecting job precedence
    public static int EvaluateMakespan(
        IReadOnlyList<JSPTask> sequence,
        IReadOnlyDictionary<string, string?> predecessorByTaskKey)
    {
        // Track completion times for jobs and machines
        Dictionary<int, int> jobCompletion = [];
        Dictionary<string, int> machineCompletion = [];
        // Track the tasks that have already been scheduled
        HashSet<string> completed = [];
        // Work through the sequence until every task is placed
        List<JSPTask> pending = [.. sequence];

        int makespan = 0;

        while (pending.Count > 0)
        {
            bool progressed = false;

            // Scan the pending list and place any task whose predecessor is complete
            for (int index = 0; index < pending.Count; index++)
            {
                JSPTask task = pending[index];
                string taskKey = CreateTaskKey(task);
                string? predecessorKey = predecessorByTaskKey[taskKey];

                if (predecessorKey is not null && !completed.Contains(predecessorKey))
                {
                    // Wait for the predecessor before scheduling this task
                    continue;
                }

                // Compute the earliest feasible start time on the job and machine
                int jobReadyTime = jobCompletion.GetValueOrDefault(task.JobId, 0);
                string machine = GetMachineKey(task.SubDivision);
                int machineReadyTime = machineCompletion.GetValueOrDefault(machine, 0);

                int start = Math.Max(jobReadyTime, machineReadyTime);
                int finish = start + task.ProcessingTime;

                // Commit the task to the schedule state
                jobCompletion[task.JobId] = finish;
                machineCompletion[machine] = finish;
                completed.Add(taskKey);
                pending.RemoveAt(index);
                makespan = Math.Max(makespan, finish);
                progressed = true;
                index--;
            }

            if (!progressed)
            {
                // Return a large penalty if the sequence cannot be scheduled
                return int.MaxValue / 2;
            }
        }

        // Return the final completion time of the whole schedule
        return makespan;
    }

    // Builds the immediate predecessor map for each task in every job
    public static Dictionary<string, string?> BuildPredecessorMap(IReadOnlyList<JSPTask> tasks)
    {
        // Map each task key to the previous operation in the same job
        Dictionary<string, string?> predecessorByTaskKey = [];

        // Group tasks by job so each job chain can be ordered by operation
        foreach (IGrouping<int, JSPTask> jobGroup in tasks.GroupBy(task => task.JobId))
        {
            // Sort each job's operations in their natural order
            List<JSPTask> orderedByOperation = [.. jobGroup.OrderBy(task => task.Operation)];
            for (int index = 0; index < orderedByOperation.Count; index++)
            {
                string key = CreateTaskKey(orderedByOperation[index]);
                string? predecessor = index == 0 ? null : CreateTaskKey(orderedByOperation[index - 1]);
                predecessorByTaskKey[key] = predecessor;
            }
        }

        // Return the predecessor lookup table
        return predecessorByTaskKey;
    }

    // Creates a key for a task
    private static string CreateTaskKey(JSPTask task)
    {
        return $"{task.JobId}:{task.Operation}";
    }

    // Normalises the machine label
    private static string GetMachineKey(string? subdivision)
    {
        return string.IsNullOrWhiteSpace(subdivision) ? "Unknown" : subdivision.Trim();
    }

    // Sums the total amount of work in a task sequence
    public static int CalculateTotalProcessingTime(IEnumerable<JSPTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        return tasks.Sum(task => task.ProcessingTime);
    }

    // Evaluates the final makespan for a task sequence, respecting job precedence
    public static int CalculateMakespan(Schedule schedule, IReadOnlyList<JSPTask> orderedTasks)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ArgumentNullException.ThrowIfNull(orderedTasks);

        Dictionary<string, string?> predecessorByTaskKey = BuildPredecessorMap(schedule.tasks);
        return EvaluateMakespan(orderedTasks, predecessorByTaskKey);
    }
}