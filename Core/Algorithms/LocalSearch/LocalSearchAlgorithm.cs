namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Base class for multi-start local search algorithms
public abstract class LocalSearchAlgorithm : ISchedulingAlgorithm
{
    // Stores the current parameters for this algorithm
    protected ILocalSearchParameters parameters;

    // All local search algorithms belong to this category
    public AlgorithmCategory Category => AlgorithmCategory.LocalSearch;

    // Subclasses define their specific algorithm identifier
    public abstract AlgorithmId Id { get; }

    // Subclasses define their display name
    public abstract string DisplayName { get; }

    // Gets the current parameters for this algorithm
    public IAlgorithmParameters Parameters => parameters;

    // Configures the algorithm with new local search parameters
    public void ConfigureParameters(IAlgorithmParameters newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);

        if (newParameters is not ILocalSearchParameters localSearchParams)
        {
            throw new ArgumentException(
                $"Parameters must be of type {nameof(ILocalSearchParameters)}, got {newParameters.GetType().Name}",
                nameof(newParameters));
        }

        string? validationError = newParameters.Validate();
        if (validationError is not null)
        {
            throw new ArgumentException(validationError, nameof(newParameters));
        }

        parameters = localSearchParams;
    }

    // Constructor with default parameters
    protected LocalSearchAlgorithm()
    {
        parameters = new LocalSearchParameters { ConfigurationName = "Default" };
    }

    // Common execution pattern for local search algorithms
    public AlgorithmExecutionResult Execute(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (schedule.tasks.Length == 0)
        {
            return new AlgorithmExecutionResult("No Tasks", "The selected schedule has no tasks.", isError: true);
        }

        Dictionary<string, string?> predecessorMap = ScheduleEvaluation.BuildPredecessorMap(schedule.tasks);

        // Generate initial sequences according to configuration
        List<(string name, List<JSPTask> sequence)> seeds = [];
        
        // Add SPT seed
        var sptAlgorithm = new ShortestProcessingTimeAlgorithm();
        seeds.Add(("SPT", [.. sptAlgorithm.BuildSequence(schedule)]));

        // Add LPT seed
        var lptAlgorithm = new LongestProcessingTimeAlgorithm();
        seeds.Add(("LPT", [.. lptAlgorithm.BuildSequence(schedule)]));

        // Add remaining random seeds up to configured total
        var randomAlgorithm = new RandomAlgorithm();
        int randomSeedsNeeded = Math.Max(0, parameters.MultiStartSeeds - 2);
        for (int i = 0; i < randomSeedsNeeded; i++)
        {
            seeds.Add(($"Random {i + 1}", [.. randomAlgorithm.BuildSequence(schedule)]));
        }

        // Run search on each seed and track the best result
        (string seedName, LocalSearchResult result)? best = null;

        foreach (var (name, sequence) in seeds)
        {
            // Apply local search from this seed
            var result = RunSearch(sequence, predecessorMap);

            // Keep track of the overall best solution across all seeds
            if (best is null || result.FinalMakespan < best.Value.result.FinalMakespan)
            {
                best = (name, result);
            }
        }

        if (best is null)
        {
            return new AlgorithmExecutionResult("Local Search Error", "No valid search results.", isError: true);
        }

        return BuildResultMessage(schedule, best.Value.seedName, best.Value.result, best.Value.result.BestSequence);
    }

    // Encapsulates the results of a local search run
    protected record LocalSearchResult(int FinalMakespan, int Iterations, int Improvements, List<JSPTask> BestSequence);

    // Subclasses implement their specific search algorithm on a single seed
    protected abstract LocalSearchResult RunSearch(List<JSPTask> sequence, Dictionary<string, string?> predecessorMap);

    // Subclasses format their specific result message
    protected abstract AlgorithmExecutionResult BuildResultMessage(Schedule schedule, string seedName, LocalSearchResult result, List<JSPTask> bestSequence);
}
