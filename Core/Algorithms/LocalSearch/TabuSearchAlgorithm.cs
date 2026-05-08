namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;

using System.Collections.Concurrent;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Tabu search local-search implementation
public class TabuSearchAlgorithm : LocalSearchAlgorithm
{
    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.TabuSearch;
    // Algorithm display name
    public override string DisplayName => "Tabu Search";

    // Override base constructor to use tabu-specific parameters
    public TabuSearchAlgorithm()
    {
        parameters = new TabuSearchParameters { ConfigurationName = "Default" };
    }

    // Allows configuration with custom tabu parameters
    public new void ConfigureParameters(IAlgorithmParameters newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);

        if (newParameters is not ITabuSearchParameters tabuParams)
        {
            throw new ArgumentException(
                $"Parameters must be of type {nameof(ITabuSearchParameters)}, got {newParameters.GetType().Name}",
                nameof(newParameters));
        }

        string? validationError = newParameters.Validate();
        if (validationError is not null)
        {
            throw new ArgumentException(validationError, nameof(newParameters));
        }

        parameters = tabuParams;
    }

    // Gets tabu search parameters cast to ITabuSearchParameters
    private ITabuSearchParameters TabuParameters => (ITabuSearchParameters)parameters;

    // Executes tabu search
    protected override LocalSearchResult RunSearch(List<JSPTask> sequence, Dictionary<string, string?> predecessorMap)
    {
        var state = InitialiseTabuSearch(sequence, predecessorMap);
        RunCriticalPathSearchIterations(state);
        List<JSPTask> repairedBest = SequenceRepair.RepairToFeasibleOrder(state.Best, state.PredecessorMap);
        return new LocalSearchResult(state.BestMakespan, state.Iterations, state.Improvements, repairedBest);
    }

    // Manages mutable state during critical path tabu search
    private class TabuSearchState
    {
        public List<JSPTask> Current { get; set; } = [];
        public List<JSPTask> Best { get; set; } = [];
        public Dictionary<string, string?> PredecessorMap { get; set; } = [];
        public Dictionary<(int, int), int> TabuUntilIteration { get; set; } = [];
        public int BestMakespan { get; set; }
        public int CurrentMakespan { get; set; }
        public int Improvements { get; set; }
        public int Iterations { get; set; }
    }

    // Initialises the tabu search state
    private static TabuSearchState InitialiseTabuSearch(List<JSPTask> seed, Dictionary<string, string?> predecessorMap)
    {
        List<JSPTask> feasibleSeed = SequenceRepair.RepairToFeasibleOrder(seed, predecessorMap);
        int initialMakespan = ScheduleEvaluation.EvaluateMakespan(feasibleSeed, predecessorMap);
        return new TabuSearchState
        {
            Current = [.. feasibleSeed],
            Best = [.. feasibleSeed],
            PredecessorMap = predecessorMap,
            TabuUntilIteration = [],
            BestMakespan = initialMakespan,
            CurrentMakespan = initialMakespan,
            Improvements = 0,
            Iterations = 0
        };
    }

    // Executes critical path based tabu search iterations
    private void RunCriticalPathSearchIterations(TabuSearchState state)
    {
        int iterationsWithoutImprovement = 0;
        int lastBestMakespan = state.BestMakespan;

        for (int iteration = 1; iteration <= parameters.MaxIterations; iteration++)
        {
            state.Iterations = iteration;

            // Calculate task timings and identify critical path
            var taskTimings = CalculateTaskTimings(state.Current, state.PredecessorMap);
            var criticalPath = IdentifyCriticalPath(state.Current, taskTimings, state.PredecessorMap);
            var criticalCandidates = GenerateCriticalPathCandidates(state.Current, criticalPath);

            if (criticalCandidates.Count == 0)
            {
                break;
            }

            // Find best admissible move on critical path
            (int fromIdx, int toIdx, List<JSPTask> sequence, int makespan)? bestMove = FindBestCriticalMove(criticalCandidates, state, iteration);

            if (bestMove is null)
            {
                break;
            }

            state.Current.Clear();
            state.Current.AddRange(bestMove.Value.sequence);
            state.Current = SequenceRepair.RepairToFeasibleOrder(state.Current, state.PredecessorMap);
            state.CurrentMakespan = ScheduleEvaluation.EvaluateMakespan(state.Current, state.PredecessorMap);
            state.TabuUntilIteration[(bestMove.Value.fromIdx, bestMove.Value.toIdx)] = iteration + TabuParameters.TabuTenure;

            if (state.CurrentMakespan < state.BestMakespan)
            {
                state.Best.Clear();
                state.Best.AddRange(state.Current);
                state.BestMakespan = state.CurrentMakespan;
                state.Improvements++;
                lastBestMakespan = state.BestMakespan;
                iterationsWithoutImprovement = 0;
            }
            else
            {
                iterationsWithoutImprovement++;
            }

            RemoveExpiredTabuEntries(state.TabuUntilIteration, iteration);

            // Early termination if no improvement
            if (TabuParameters.MaxIterationsWithoutImprovement > 0 && 
                iterationsWithoutImprovement >= TabuParameters.MaxIterationsWithoutImprovement)
            {
                break;
            }
        }
    }

    // Records task timing (start and end cumulative hours)
    private record TaskTiming(int SequenceIndex, string Machine, int StartTime, int EndTime, int JobId);

    // Calculates start/end times for each task in the current sequence
    private static List<TaskTiming> CalculateTaskTimings(
        IReadOnlyList<JSPTask> sequence,
        IReadOnlyDictionary<string, string?> predecessorMap)
    {
        var timings = new List<TaskTiming>();
        var jobCompletion = new Dictionary<int, int>();
        var machineCompletion = new Dictionary<string, int>();
        var completed = new HashSet<string>();
        var pending = new List<JSPTask>([.. sequence]);

        int sequenceIndex = 0;
        while (pending.Count > 0 && sequenceIndex < sequence.Count)
        {
            bool progressed = false;
            for (int i = 0; i < pending.Count; i++)
            {
                var task = pending[i];
                string taskKey = $"{task.JobId}:{task.Operation}";
                string? predKey = predecessorMap[taskKey];

                if (predKey is not null && !completed.Contains(predKey))
                {
                    continue;
                }

                string machine = task.SubDivision ?? "Unknown";
                int jobReady = jobCompletion.GetValueOrDefault(task.JobId, 0);
                int machineReady = machineCompletion.GetValueOrDefault(machine, 0);
                int start = Math.Max(jobReady, machineReady);
                int end = start + task.ProcessingTime;

                timings.Add(new TaskTiming(sequenceIndex, machine, start, end, task.JobId));
                jobCompletion[task.JobId] = end;
                machineCompletion[machine] = end;
                completed.Add(taskKey);
                pending.RemoveAt(i);
                progressed = true;
                sequenceIndex++;
                break;
            }

            if (!progressed) break;
        }

        return timings;
    }

    // Identifies tasks on the critical path (longest path to makespan)
    private static HashSet<int> IdentifyCriticalPath(
        IReadOnlyList<JSPTask> sequence,
        List<TaskTiming> timings,
        IReadOnlyDictionary<string, string?> predecessorMap)
    {
        if (timings.Count == 0) return [];

        int makespan = timings.Max(t => t.EndTime);
        var critical = new HashSet<int>();
        var criticalTasks = timings.Where(t => t.EndTime == makespan).ToList();

        foreach (var task in criticalTasks)
        {
            BacktrackCriticalPath(task, timings, sequence, predecessorMap, critical, makespan);
        }

        return critical;
    }

    // Backtracks to mark all tasks on the critical path
    private static void BacktrackCriticalPath(
        TaskTiming task,
        List<TaskTiming> timings,
        IReadOnlyList<JSPTask> sequence,
        IReadOnlyDictionary<string, string?> predecessorMap,
        HashSet<int> critical,
        int makespan)
    {
        if (critical.Contains(task.SequenceIndex)) return;
        critical.Add(task.SequenceIndex);

        var seqTask = sequence[task.SequenceIndex];
        string taskKey = $"{seqTask.JobId}:{seqTask.Operation}";
        string? predKey = predecessorMap[taskKey];

        // Find and mark predecessor on critical path
        if (predKey is not null)
        {
            var predTiming = timings.FirstOrDefault(t => 
                t.EndTime == task.StartTime && 
                sequence[t.SequenceIndex].JobId == seqTask.JobId &&
                sequence[t.SequenceIndex].Operation == seqTask.Operation - 1);
            if (predTiming != null)
            {
                BacktrackCriticalPath(predTiming, timings, sequence, predecessorMap, critical, makespan);
            }
        }

        // Find machine predecessor on critical path
        var machinePrec = timings
            .FirstOrDefault(t => t.Machine == task.Machine && t.EndTime == task.StartTime && t.SequenceIndex != task.SequenceIndex);
        if (machinePrec != null)
        {
            BacktrackCriticalPath(machinePrec, timings, sequence, predecessorMap, critical, makespan);
        }
    }

    // Generates swap candidates for adjacent pairs on the critical path sharing a machine
    private static List<(int fromIdx, int toIdx, List<JSPTask> candidate)> GenerateCriticalPathCandidates(
        IReadOnlyList<JSPTask> sequence,
        HashSet<int> criticalPath)
    {
        var candidates = new List<(int, int, List<JSPTask>)>();

        var criticalList = criticalPath.OrderBy(i => i).ToList();
        for (int i = 0; i < criticalList.Count - 1; i++)
        {
            int idx1 = criticalList[i];
            int idx2 = criticalList[i + 1];

            // Check if adjacent on critical path and share a machine
            if (idx2 == idx1 + 1 && sequence[idx1].SubDivision == sequence[idx2].SubDivision)
            {
                var candidate = new List<JSPTask>(sequence);
                (candidate[idx1], candidate[idx2]) = (candidate[idx2], candidate[idx1]);
                candidates.Add((idx1, idx2, candidate));
            }
        }

        return candidates;
    }

    // Finds the best non-tabu move or any move that improves the best-ever makespan
    private static (int fromIdx, int toIdx, List<JSPTask> sequence, int makespan)? FindBestCriticalMove(
        List<(int fromIdx, int toIdx, List<JSPTask> candidate)> candidates,
        TabuSearchState state,
        int iteration)
    {
        // Evaluate all candidates in parallel to find the best non-tabu move
        var evaluatedCandidates = new ConcurrentBag<(int fromIdx, int toIdx, List<JSPTask> repaired, int makespan)>();
        object tabuLock = new();

        Parallel.ForEach(candidates, candidate =>
        {
            int fromIdx = candidate.fromIdx;
            int toIdx = candidate.toIdx;
            List<JSPTask> candidateSeq = candidate.candidate;
            
            // Repair sequence to maintain job precedence
            var repaired = SequenceRepair.RepairToFeasibleOrder(candidateSeq, state.PredecessorMap);
            // Evaluate the candidate solution
            int candidateMakespan = ScheduleEvaluation.EvaluateMakespan(repaired, state.PredecessorMap);

            // Check tabu status and aspiration criteria
            bool isTabu = false;
            bool aspirationSatisfied = candidateMakespan < state.BestMakespan;
            
            lock (tabuLock)
            {
                isTabu = state.TabuUntilIteration.TryGetValue((fromIdx, toIdx), out int tabuExpiry) && tabuExpiry >= iteration;
            }

            // Only add if not tabu or aspiration satisfied
            if (!isTabu || aspirationSatisfied)
            {
                evaluatedCandidates.Add((fromIdx, toIdx, repaired, candidateMakespan));
            }
        });

        // Find the best move from evaluated candidates
        (int fromIdx, int toIdx, List<JSPTask> sequence, int makespan)? bestMove = null;
        foreach (var candidate in evaluatedCandidates)
        {
            if (bestMove is null || candidate.makespan < bestMove.Value.makespan)
            {
                bestMove = (candidate.fromIdx, candidate.toIdx, candidate.repaired, candidate.makespan);
            }
        }

        return bestMove;
    }


    // Removes expired tabu entries
    private static void RemoveExpiredTabuEntries(Dictionary<(int, int), int> tabuUntilIteration, int iteration)
    {
        var expired = tabuUntilIteration
            .Where(entry => entry.Value < iteration)
            .Select(entry => entry.Key)
            .ToList();

        foreach (var key in expired)
        {
            tabuUntilIteration.Remove(key);
        }
    }

    // Builds the result message
    protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, string seedName, LocalSearchResult result, List<JSPTask> bestSequence)
    {
        string message = AlgorithmResultFormatter.BuildStandardMessage(
            schedule,
            DisplayName,
            schedule.tasks.Length,
            result.FinalMakespan);

        return new AlgorithmExecutionResult(
            "Tabu Search Result",
            message,
            computedSchedule: bestSequence,
            makespan: result.FinalMakespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }
}