namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Shared utility for repairing task sequences to satisfy job precedence constraints
public static class SequenceRepair
{
    // Creates a unique key for a task in format "JobId:Operation"
    public static string CreateTaskKey(JSPTask task)
    {
        return $"{task.JobId}:{task.Operation}";
    }

    // Repairs a task sequence to ensure all job precedence constraints are satisfied
    // Tasks are reordered so that predecessors always appear before successors
    public static List<JSPTask> RepairToFeasibleOrder(
        IReadOnlyList<JSPTask> sequence,
        IReadOnlyDictionary<string, string?> predecessorMap)
    {
        var pred = new Dictionary<string, string?>(predecessorMap);
        var repaired = new List<JSPTask>();
        var completed = new HashSet<string>();
        var pending = new Queue<JSPTask>(sequence);

        while (pending.Count > 0)
        {
            var next = pending.Dequeue();
            string taskKey = CreateTaskKey(next);
            string? predKey = pred.TryGetValue(taskKey, out string? p) ? p : null;

            if (predKey is not null && !completed.Contains(predKey))
            {
                // Predecessor not yet scheduled, put this task back in the queue
                pending.Enqueue(next);
            }
            else
            {
                // Predecessor is done (or there is no predecessor), add to repaired sequence
                repaired.Add(next);
                completed.Add(taskKey);
            }
        }

        return repaired;
    }
}
