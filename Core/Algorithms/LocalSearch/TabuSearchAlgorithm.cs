namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;

using System.Diagnostics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Tabu search local-search implementation with optional wider neighborhoods
public class TabuSearchAlgorithm : LocalSearchAlgorithm
{
    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.TabuSearch;
    // Algorithm display name
    public override string DisplayName => "Tabu Search";

    private readonly int tabuTenure;
    private readonly int maxIterations;
    private readonly bool useDoubleNeighborhoods;
    private readonly int diversificationThreshold;

    // Configures the tabu search limits and neighborhood behavior
    public TabuSearchAlgorithm(int tabuTenure = 7, int maxIterations = 500, bool useDoubleNeighborhoods = false, int diversificationThreshold = 15)
    {
        if (tabuTenure <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tabuTenure), "Tabu tenure must be greater than zero.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be greater than zero.");
        }

        if (diversificationThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(diversificationThreshold), "Diversification threshold must be greater than zero.");
        }

        this.tabuTenure = tabuTenure;
        this.maxIterations = maxIterations;
        this.useDoubleNeighborhoods = useDoubleNeighborhoods;
        this.diversificationThreshold = diversificationThreshold;
    }

    // Executes tabu search on a single seed sequence
    protected override LocalSearchResult RunSearch(List<JSPTask> sequence, Dictionary<string, string?> predecessorMap)
    {
        var state = InitialiseTabuSearch(sequence, predecessorMap);
        RunSearchIterations(state);
        return new LocalSearchResult(state.BestMakespan, state.Iterations, state.Improvements);
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
        for (int iteration = 1; iteration <= maxIterations; iteration++)
        {
            state.Iterations = iteration;
            var candidates = (useDoubleNeighborhoods && state.UsingAnyPair)
                ? LocalSearchNeighborhood.GenerateAnyPairSwapCandidates(state.Current)
                : LocalSearchNeighborhood.GenerateAdjacentSwapCandidates(state.Current);

            (LocalSearchNeighborhood.AdjacentSwapMove move, List<JSPTask> sequence, int makespan)? bestNeighbor = FindBestAdmissibleMove(candidates, state, iteration);

            if (bestNeighbor is null)
            {
                break;
            }

            state.Current.Clear();
            state.Current.AddRange(bestNeighbor.Value.sequence);
            state.TabuUntilIteration[bestNeighbor.Value.move] = iteration + tabuTenure;
            state.CurrentMakespan = bestNeighbor.Value.makespan;

            UpdateBestSolution(state);
            RemoveExpiredTabuEntries(state.TabuUntilIteration, iteration);
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
    private void UpdateBestSolution(TabuSearchState state)
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
            if (useDoubleNeighborhoods && !state.UsingAnyPair && state.StuckIterations >= diversificationThreshold)
            {
                state.UsingAnyPair = true;
            }
        }
    }

    // Builds the result message summarising the search outcome
    protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, string seedName, LocalSearchResult result)
    {
        string message =
            $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\n" +
            $"Algorithm: {DisplayName}\n" +
            "Objective: Minimise makespan\n" +
            $"Task count: {schedule.tasks.Length}\n" +
            $"Best seed: {seedName}\n" +
            $"Final makespan: {result.finalMakespan}\n" +
            $"Max iterations: {maxIterations}\n" +
            $"Tabu tenure: {tabuTenure}\n" +
            $"Double neighborhoods: {(useDoubleNeighborhoods ? "Enabled" : "Disabled")}\n" +
            $"Improvements accepted: {result.improvements}";

        return new AlgorithmExecutionResult("Tabu Search Result", message);
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