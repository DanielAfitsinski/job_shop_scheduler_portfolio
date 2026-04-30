namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Implementations;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Ordered Crossover (OX) operator for task sequences
public class OrderedCrossoverOperator : ICrossoverOperator
{
    public string Name => "Ordered Crossover";

    public List<JSPTask> Crossover(IReadOnlyList<JSPTask> parentA, IReadOnlyList<JSPTask> parentB)
    {
        int length = parentA.Count;
        int start = Random.Shared.Next(length);
        int end = Random.Shared.Next(start, length);

        // Fill the child with the preserved slice and then backfill from the second parent
        JSPTask?[] child = new JSPTask?[length];
        HashSet<string> inherited = [];

        for (int index = start; index <= end; index++)
        {
            JSPTask task = parentA[index];
            child[index] = task;
            inherited.Add(CreateTaskKey(task));
        }

        int insertIndex = 0;
        foreach (JSPTask task in parentB)
        {
            string key = CreateTaskKey(task);
            if (inherited.Contains(key))
            {
                // Skip tasks that already came from the preserved slice
                continue;
            }

            while (insertIndex < length && child[insertIndex] is not null)
            {
                // Find the next empty child slot
                insertIndex++;
            }

            if (insertIndex < length)
            {
                // Place the remaining tasks in parentB order
                child[insertIndex] = task;
            }
        }

        // Return the fully assembled child sequence
        return [.. child.Where(task => task is not null).Select(task => task!)];
    }

    // Creates a unique identifier for a task (job-operation pair)
    private static string CreateTaskKey(JSPTask task) => $"{task.JobId}:{task.Operation}";
}
