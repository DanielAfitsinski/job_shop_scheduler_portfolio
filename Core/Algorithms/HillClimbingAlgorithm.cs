using System.Diagnostics;

public class HillClimbingAlgorithm : ISchedulingAlgorithm
{
    public AlgorithmCategory Category => AlgorithmCategory.LocalSearch;
    public AlgorithmId Id => AlgorithmId.HillClimbing;
    public string DisplayName => "Hill Climbing";

    public AlgorithmExecutionResult Execute(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (schedule.tasks.Length == 0)
        {
            return new AlgorithmExecutionResult("No Tasks", "The selected schedule has no tasks.", isError: true);
        }

        List<JSPTask> current = [.. ShortestProcessingTimeAlgorithm.BuildSequence(schedule)];
        Dictionary<string, string?> predecessorByTaskKey = BuildPredecessorMap(schedule.tasks);

        int initialMakespan = EvaluateMakespan(current, predecessorByTaskKey);
        int currentMakespan = initialMakespan;

        int iterations = 0;
        int improvements = 0;
        const int maxIterations = 200;

        Stopwatch stopwatch = Stopwatch.StartNew();

        while (iterations < maxIterations)
        {
            iterations++;
            bool foundImprovement = false;

            for (int index = 0; index < current.Count - 1; index++)
            {
                List<JSPTask> candidate = [.. current];
                (candidate[index], candidate[index + 1]) = (candidate[index + 1], candidate[index]);

                int candidateMakespan = EvaluateMakespan(candidate, predecessorByTaskKey);
                if (candidateMakespan < currentMakespan)
                {
                    current = candidate;
                    currentMakespan = candidateMakespan;
                    improvements++;
                    foundImprovement = true;
                    break;
                }
            }

            if (!foundImprovement)
            {
                break;
            }
        }

        stopwatch.Stop();

        string message =
            $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\n" +
            $"Algorithm: {DisplayName}\n" +
            "Objective: Minimize makespan\n" +
            $"Task count: {schedule.tasks.Length}\n" +
            $"Initial makespan (SPT seed): {initialMakespan}\n" +
            $"Final makespan: {currentMakespan}\n" +
            $"Iterations: {iterations}\n" +
            $"Improvements accepted: {improvements}\n" +
            $"Elapsed: {stopwatch.ElapsedMilliseconds} ms";

        return new AlgorithmExecutionResult("Hill Climbing Result", message);
    }

    private static int EvaluateMakespan(IReadOnlyList<JSPTask> sequence, IReadOnlyDictionary<string, string?> predecessorByTaskKey)
    {
        Dictionary<int, int> jobCompletion = [];
        Dictionary<string, int> machineCompletion = [];
        HashSet<string> completed = [];
        List<JSPTask> pending = [.. sequence];

        int makespan = 0;

        while (pending.Count > 0)
        {
            bool progressed = false;

            for (int index = 0; index < pending.Count; index++)
            {
                JSPTask task = pending[index];
                string taskKey = CreateTaskKey(task);
                string? predecessorKey = predecessorByTaskKey[taskKey];

                if (predecessorKey is not null && !completed.Contains(predecessorKey))
                {
                    continue;
                }

                int jobReadyTime = jobCompletion.GetValueOrDefault(task.JobId, 0);
                string machine = GetMachineKey(task.SubDivision);
                int machineReadyTime = machineCompletion.GetValueOrDefault(machine, 0);

                int start = Math.Max(jobReadyTime, machineReadyTime);
                int finish = start + task.ProcessingTime;

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
                return int.MaxValue / 2;
            }
        }

        return makespan;
    }

    private static Dictionary<string, string?> BuildPredecessorMap(IReadOnlyList<JSPTask> tasks)
    {
        Dictionary<string, string?> predecessorByTaskKey = [];

        foreach (IGrouping<int, JSPTask> jobGroup in tasks.GroupBy(task => task.JobId))
        {
            List<JSPTask> orderedByOperation = [.. jobGroup.OrderBy(task => task.Operation)];
            for (int index = 0; index < orderedByOperation.Count; index++)
            {
                string key = CreateTaskKey(orderedByOperation[index]);
                string? predecessor = index == 0 ? null : CreateTaskKey(orderedByOperation[index - 1]);
                predecessorByTaskKey[key] = predecessor;
            }
        }

        return predecessorByTaskKey;
    }

    private static string CreateTaskKey(JSPTask task)
    {
        return $"{task.JobId}:{task.Operation}";
    }

    private static string GetMachineKey(string? subdivision)
    {
        return string.IsNullOrWhiteSpace(subdivision) ? "Unknown" : subdivision.Trim();
    }
}