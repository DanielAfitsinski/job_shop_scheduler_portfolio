namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;

using System.Diagnostics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Memetic algorithm that combines GA evolution with local search refinement
public class MemeticHybridAlgorithm : GeneticAlgorithm
{
    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.MemeticHybrid;
    // Algorithm display name
    public override string DisplayName => "Memetic Hybrid";

    private readonly int localSearchIterations;

    // Configures the memetic hybrid population, mutation, and local search settings
    public MemeticHybridAlgorithm(
        int populationSize = 25,
        int generations = 60,
        double mutationRate = 0.20,
        int eliteCount = 2,
        int tournamentSize = 3,
        int localSearchIterations = 50)
        : base(populationSize, generations, mutationRate, eliteCount, tournamentSize)
    {
        if (localSearchIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(localSearchIterations), "Local search iterations must be greater than zero");
        }

        this.localSearchIterations = localSearchIterations;
    }

    // Executes the memetic hybrid using the base Execute() pattern with local search integration
    protected override void EvolvePopulation(EvolutionState state)
    {
        EvolveGenerationsWithLocalSearch(state);
    }

    // Returns effective population and generation sizes for large schedules
    protected override (int populationSize, int generations) GetEffectiveSizes(int taskCount)
    {
        int effectivePopulation = taskCount > 120 ? Math.Min(populationSize, 18) : populationSize;
        int effectiveGenerations = taskCount > 120 ? Math.Min(generations, 35) : generations;
        return (effectivePopulation, effectiveGenerations);
    }

    // Builds the result message summarising the memetic search
    protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, EvolutionState state)
    {
        string message =
            $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\n" +
            $"Algorithm: {DisplayName}\n" +
            "Objective: Minimise makespan\n" +
            $"Task count: {state.TaskCount}\n" +
            $"Initial makespan (SPT seed): {state.InitialMakespan}\n" +
            $"Final makespan: {state.BestMakespan}\n" +
            $"Population size: {state.EffectivePopulationSize}\n" +
            $"Generations: {state.EffectiveGenerations}\n" +
            $"Mutation rate: {mutationRate:P0}\n" +
            $"Local search applications: {state.LocalSearchApplications}\n" +
            $"Evaluations: {state.Evaluations}";

        return new AlgorithmExecutionResult(
            "Memetic Hybrid Result",
            message,
            computedSchedule: state.BestSequence,
            makespan: state.BestMakespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }

    // Executes the main memetic evolutionary loop with local search refinement
    private void EvolveGenerationsWithLocalSearch(EvolutionState state)
    {
        for (int generation = 0; generation < state.EffectiveGenerations; generation++)
        {
            int evaluations = state.Evaluations;
            var scoredPopulation = ScorePopulation(state.Population, state.PredecessorMap, ref evaluations);
            state.Evaluations = evaluations;

            var generationBest = scoredPopulation[0];
            if (generationBest.Makespan < state.BestMakespan)
            {
                state.BestMakespan = generationBest.Makespan;
                state.BestSequence.Clear();
                state.BestSequence.AddRange(generationBest.Sequence);
            }

            var nextPopulation = new List<List<JSPTask>>();

            for (int eliteIndex = 0; eliteIndex < eliteCount; eliteIndex++)
            {
                var eliteSequence = new List<JSPTask>(scoredPopulation[eliteIndex].Sequence);
                int evals = state.Evaluations;
                int lsApps = state.LocalSearchApplications;
                var improvedElite = ApplyLocalSearch(eliteSequence, state.PredecessorMap, ref evals, ref lsApps);
                state.Evaluations = evals;
                state.LocalSearchApplications = lsApps;
                nextPopulation.Add(improvedElite);
            }

            while (nextPopulation.Count < state.EffectivePopulationSize)
            {
                var parentA = TournamentSelect(scoredPopulation);
                var parentB = TournamentSelect(scoredPopulation);

                var child = Crossover(parentA, parentB);

                if (Random.Shared.NextDouble() < mutationRate)
                {
                    Mutate(child);
                }

                var repairedChild = RepairToFeasibleOrder(child, state.PredecessorMap);
                int evals2 = state.Evaluations;
                int lsApps2 = state.LocalSearchApplications;
                var improvedChild = ApplyLocalSearch(repairedChild, state.PredecessorMap, ref evals2, ref lsApps2);
                state.Evaluations = evals2;
                state.LocalSearchApplications = lsApps2;
                nextPopulation.Add(improvedChild);
            }

            state.Population.Clear();
            state.Population.AddRange(nextPopulation);
        }
    }

    // Applies hill-climbing local search to improve a candidate solution
    private List<JSPTask> ApplyLocalSearch(
        List<JSPTask> sequence,
        IReadOnlyDictionary<string, string?> predecessorByTaskKey,
        ref int evaluations,
        ref int localSearchApplications)
    {
        List<JSPTask> current = [.. sequence];
        int currentMakespan = ScheduleEvaluation.EvaluateMakespan(current, predecessorByTaskKey);
        evaluations++;

        // Run hill-climbing iterations until no improvement is found
        int iterations = 0;
        while (iterations < localSearchIterations)
        {
            iterations++;
            bool foundImprovement = false;

            // Try every adjacent swap until an improvement is found
            foreach ((_, List<JSPTask> candidate) in LocalSearchNeighborhood.GenerateAdjacentSwapCandidates(current))
            {
                int candidateMakespan = ScheduleEvaluation.EvaluateMakespan(candidate, predecessorByTaskKey);
                evaluations++;

                if (candidateMakespan < currentMakespan)
                {
                    // Accept the first strictly better candidate
                    current = candidate;
                    currentMakespan = candidateMakespan;
                    foundImprovement = true;
                    break;
                }
            }

            if (!foundImprovement)
            {
                // Stop when the neighborhood contains no better solution
                break;
            }
        }

        localSearchApplications++;
        return current;
    }
}
