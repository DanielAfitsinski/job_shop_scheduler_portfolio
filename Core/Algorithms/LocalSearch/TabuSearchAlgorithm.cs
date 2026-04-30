namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;

using System.Diagnostics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Tabu search local-search implementation with optional wider neighborhoods
public class TabuSearchAlgorithm : LocalSearchAlgorithm
{
    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.TabuSearch;
    // Algorithm display name
    public override string DisplayName => "Tabu Search";

    // Threshold for applying diversification strategy
    private const int DiversificationThreshold = 15;

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

    // Executes tabu search on a single seed sequence
    protected override LocalSearchResult RunSearch(List<JSPTask> sequence, Dictionary<string, string?> predecessorMap)
    {
        var state = InitialiseTabuSearch(sequence, predecessorMap);
        RunSearchIterations(state);
        return new LocalSearchResult(state.BestMakespan, state.Iterations, state.Improvements, state.Best);
    }

    // Manages mutable state during tabu search execution
    private class TabuSearchState
    {
        public List<JSPTask> Current { get; set; } = [];
        public List<JSPTask> Best { get; set; } = [];
        public Dictionary<string, string?> PredecessorMap { get; set; } = [];
        public Dictionary<LocalSearchNeighborhood.AdjacentSwapMove, int> TabuUntilIteration { get; set; } = [];
        public int BestMakespan { get; set; }
        public int CurrentMakespan { get; set; }
        public int Improvements { get; set; }
        public int Iterations { get; set; }
        public int StuckIterations { get; set; }
        public bool UsingAnyPair { get; set; }
        public Stopwatch Stopwatch { get; set; } = new();
    }

    // Creates the initial tabu search state from a seed sequence
    private static TabuSearchState InitialiseTabuSearch(List<JSPTask> seed, Dictionary<string, string?> predecessorMap)
    {
        int initialMakespan = ScheduleEvaluation.EvaluateMakespan(seed, predecessorMap);

        return new TabuSearchState
        {
            Current = [.. seed],
            Best = [.. seed],
            PredecessorMap = predecessorMap,
            TabuUntilIteration = [],
            BestMakespan = initialMakespan,
            CurrentMakespan = initialMakespan,
            Improvements = 0,
            Iterations = 0,
            StuckIterations = 0,
            UsingAnyPair = false,
            Stopwatch = Stopwatch.StartNew()
        };
    }

    // Executes the main tabu search loop for all iterations
    private void RunSearchIterations(TabuSearchState state)
    {
        int iterationsWithoutImprovement = 0;
        int lastBestMakespan = state.BestMakespan;

        for (int iteration = 1; iteration <= parameters.MaxIterations; iteration++)
        {
            state.Iterations = iteration;
            var candidates = (state.UsingAnyPair)
                ? LocalSearchNeighborhood.GenerateAnyPairSwapCandidates(state.Current)
                : LocalSearchNeighborhood.GenerateAdjacentSwapCandidates(state.Current);

            (LocalSearchNeighborhood.AdjacentSwapMove move, List<JSPTask> sequence, int makespan)? bestNeighbor = FindBestAdmissibleMove(candidates, state, iteration);

            if (bestNeighbor is null)
            {
                break;
            }

            state.Current.Clear();
            state.Current.AddRange(bestNeighbor.Value.sequence);
            state.TabuUntilIteration[bestNeighbor.Value.move] = iteration + TabuParameters.TabuTenure;
            state.CurrentMakespan = bestNeighbor.Value.makespan;

            UpdateBestSolution(state);
            RemoveExpiredTabuEntries(state.TabuUntilIteration, iteration);

            // Track iterations without improvement for early termination
            if (state.BestMakespan < lastBestMakespan)
            {
                lastBestMakespan = state.BestMakespan;
                iterationsWithoutImprovement = 0;
            }
            else
            {
                iterationsWithoutImprovement++;
            }

            // Early termination if no improvement found for configured number of iterations
            if (TabuParameters.MaxIterationsWithoutImprovement > 0 && 
                iterationsWithoutImprovement >= TabuParameters.MaxIterationsWithoutImprovement)
            {
                break;
            }
        }

        state.Stopwatch.Stop();
    }

    // Finds the best admissible neighbor move that is not tabu or satisfies aspiration
    private static (LocalSearchNeighborhood.AdjacentSwapMove move, List<JSPTask> sequence, int makespan)? FindBestAdmissibleMove(
        IEnumerable<(LocalSearchNeighborhood.AdjacentSwapMove move, List<JSPTask> candidate)> candidates,
        TabuSearchState state,
        int iteration)
    {
        (LocalSearchNeighborhood.AdjacentSwapMove move, List<JSPTask> sequence, int makespan)? bestNeighbor = null;

        foreach ((LocalSearchNeighborhood.AdjacentSwapMove move, List<JSPTask> candidate) in candidates)
        {
            int candidateMakespan = ScheduleEvaluation.EvaluateMakespan(candidate, state.PredecessorMap);
            bool isTabu = state.TabuUntilIteration.TryGetValue(move, out int tabuExpiry) && tabuExpiry >= iteration;
            bool aspirationSatisfied = candidateMakespan < state.BestMakespan;

            if (isTabu && !aspirationSatisfied)
            {
                continue;
            }

            if (bestNeighbor is null || candidateMakespan < bestNeighbor.Value.makespan)
            {
                bestNeighbor = (move, candidate, candidateMakespan);
            }
        }

        return bestNeighbor;
    }

    // Updates best solution and diversification state if current improves best
    private static void UpdateBestSolution(TabuSearchState state)
    {
        if (state.CurrentMakespan < state.BestMakespan)
        {
            state.Best.Clear();
            state.Best.AddRange(state.Current);
            state.BestMakespan = state.CurrentMakespan;
            state.Improvements++;
            state.StuckIterations = 0;
            state.UsingAnyPair = false;
        }
        else
        {
            state.StuckIterations++;
            if (!state.UsingAnyPair && state.StuckIterations >= DiversificationThreshold)
            {
                state.UsingAnyPair = true;
            }
        }
    }

    // Builds the result message summarising the search outcome
    protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, string seedName, LocalSearchResult result, List<JSPTask> bestSequence)
    {
        string message =
            $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\n" +
            $"Algorithm: {DisplayName}\n" +
            "Objective: Minimise makespan\n" +
            $"Task count: {schedule.tasks.Length}\n" +
            $"Best seed: {seedName}\n" +
            $"Final makespan: {result.FinalMakespan}\n" +
            $"Max iterations: {parameters.MaxIterations}\n" +
            $"Tabu tenure: {TabuParameters.TabuTenure}\n" +
            $"Improvements accepted: {result.Improvements}";

        return new AlgorithmExecutionResult(
            "Tabu Search Result",
            message,
            computedSchedule: bestSequence,
            makespan: result.FinalMakespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }

    // Removes tabu moves that are no longer active
    private static void RemoveExpiredTabuEntries(Dictionary<LocalSearchNeighborhood.AdjacentSwapMove, int> tabuUntilIteration, int iteration)
    {
        // Collect expired tabu moves before modifying the dictionary
        List<LocalSearchNeighborhood.AdjacentSwapMove> expiredMoves = [.. tabuUntilIteration
            .Where(entry => entry.Value < iteration)
            .Select(entry => entry.Key)];

        // Drop each expired move from the tabu list
        foreach (LocalSearchNeighborhood.AdjacentSwapMove move in expiredMoves)
        {
            tabuUntilIteration.Remove(move);
        }
    }
}