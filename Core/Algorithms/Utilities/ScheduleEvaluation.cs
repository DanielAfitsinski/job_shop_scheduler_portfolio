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

        // Process tasks in sequence, respecting precedence and resource constraints
        // Each iteration of the while loop tries to schedule one more task
        while (pending.Count > 0)
        {
            bool progressed = false;

            // Scan pending list and place any task whose predecessor is complete
            // Tasks must respect job precedence
            for (int index = 0; index < pending.Count; index++)
            {
                JSPTask task = pending[index];
                string taskKey = CreateTaskKey(task);
                string? predecessorKey = predecessorByTaskKey[taskKey];

                // Check precedence constraint: if this task has a predecessor, it must be done first
                if (predecessorKey is not null && !completed.Contains(predecessorKey))
                {
                    // Skip this task for now, try again in the next iteration
                    continue;
                }

                // Both constraints satisfied: precedence OK and predecessor is complete
                // Now compute when this task can actually start on both the job and machine
                int jobReadyTime = jobCompletion.GetValueOrDefault(task.JobId, 0);
                string machine = GetMachineKey(task.SubDivision);
                int machineReadyTime = machineCompletion.GetValueOrDefault(machine, 0);

                // Task starts at the maximum of job-ready and machine-ready times
                // Both resources must be available before work can begin
                int start = Math.Max(jobReadyTime, machineReadyTime);
                int finish = start + task.ProcessingTime;

                // Update resource availability: both job and machine are now occupied until finish time
                jobCompletion[task.JobId] = finish;
                machineCompletion[machine] = finish;
                // Mark this task as complete so other tasks can check it as a predecessor
                completed.Add(taskKey);
                // Remove from pending since it's scheduled
                pending.RemoveAt(index);
                // Track the maximum completion time across all tasks
                makespan = Math.Max(makespan, finish);
                progressed = true;
                // Back up index since we removed an element from the list we're iterating
                index--;
            }

            if (!progressed)
            {
                // No task could be scheduled despite pending tasks remaining
                // Return a large penalty value to signal this is a bad schedule
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
        // In job shop scheduling, each job must execute its operations in the specified sequence
        foreach (IGrouping<int, JSPTask> jobGroup in tasks.GroupBy(task => task.JobId))
        {
            // Sort each job's operations in their natural order (by operation number)
            // This ensures operations are processed in the correct sequence for precedence tracking
            List<JSPTask> orderedByOperation = [.. jobGroup.OrderBy(task => task.Operation)];
            
            // For each operation in this job, record what operation must complete first
            for (int index = 0; index < orderedByOperation.Count; index++)
            {
                string key = CreateTaskKey(orderedByOperation[index]);
                // The predecessor is the operation with index-1, or null if this is the first operation
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