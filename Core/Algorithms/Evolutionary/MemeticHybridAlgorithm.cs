namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;

using System.Diagnostics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
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

    // Iterations for local search refinement on elite and offspring
    private readonly int localSearchIterations = 50;

    // Builds the result message summarising the memetic search
    protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, EvolutionState state)
    {
        string message = AlgorithmResultFormatter.BuildStandardMessage(
            schedule,
            DisplayName,
            state.TaskCount,
            state.BestMakespan);

        return new AlgorithmExecutionResult(
            "Memetic Hybrid Result",
            message,
            computedSchedule: state.BestSequence,
            makespan: state.BestMakespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }

    // Executes the evolutionary loop with local search refinement
    protected override void EvolvePopulation(EvolutionState state)
    {
        EvolveGenerationsWithLocalSearch(state);
    }

    // Executes generations with local search refinement
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

            for (int eliteIndex = 0; eliteIndex < parameters.EliteCount; eliteIndex++)
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

                if (Random.Shared.NextDouble() < parameters.MutationRate)
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
