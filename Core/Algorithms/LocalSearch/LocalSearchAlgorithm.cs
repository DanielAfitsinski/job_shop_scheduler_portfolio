namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Base class for multi-start local search algorithms
public abstract class LocalSearchAlgorithm : ISchedulingAlgorithm
{
    // All local search algorithms belong to this category
    public AlgorithmCategory Category => AlgorithmCategory.LocalSearch;

    // Subclasses define their specific algorithm identifier
    public abstract AlgorithmId Id { get; }

    // Subclasses define their display name
    public abstract string DisplayName { get; }

    // Common multi-start execution pattern for all local search algorithms
    public AlgorithmExecutionResult Execute(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (schedule.tasks.Length == 0)
        {
            return new AlgorithmExecutionResult("No Tasks", "The selected schedule has no tasks.", isError: true);
        }

        Dictionary<string, string?> predecessorMap = ScheduleEvaluation.BuildPredecessorMap(schedule.tasks);

        // Generate initial sequences: 1 SPT + 1 LPT + 3 random
        List<(string name, List<JSPTask> sequence)> seeds = [];
        
        // Add SPT seed
        var sptAlgorithm = new ShortestProcessingTimeAlgorithm();
        seeds.Add(("SPT", [.. sptAlgorithm.BuildSequence(schedule)]));

        // Add LPT seed
        var lptAlgorithm = new LongestProcessingTimeAlgorithm();
        seeds.Add(("LPT", [.. lptAlgorithm.BuildSequence(schedule)]));

        // Add 3 random seeds
        var randomAlgorithm = new RandomAlgorithm();
        for (int i = 0; i < 3; i++)
        {
            seeds.Add(($"Random {i + 1}", [.. randomAlgorithm.BuildSequence(schedule)]));
        }

        // Run search on each seed and track the best result
        (string seedName, LocalSearchResult result)? best = null;

        foreach (var (name, sequence) in seeds)
        {
            var result = RunSearch(sequence, predecessorMap);

            if (best is null || result.finalMakespan < best.Value.result.finalMakespan)
            {
                best = (name, result);
            }
        }

        if (best is null)
        {
            return new AlgorithmExecutionResult("Local Search Error", "No valid search results.", isError: true);
        }

        return BuildResultMessage(schedule, best.Value.seedName, best.Value.result);
    }

    // Encapsulates the results of a local search run
    protected record LocalSearchResult(int finalMakespan, int iterations, int improvements);

    // Subclasses implement their specific search algorithm on a single seed
    protected abstract LocalSearchResult RunSearch(List<JSPTask> sequence, Dictionary<string, string?> predecessorMap);

    // Subclasses format their specific result message
    protected abstract AlgorithmExecutionResult BuildResultMessage(Schedule schedule, string seedName, LocalSearchResult result);
}
