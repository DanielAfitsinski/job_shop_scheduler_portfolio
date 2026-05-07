namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;

using System.Collections.Concurrent;
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

    // Iterations for local search refinement on elite
    private readonly int localSearchIterations = 50;
    // Iterations for local search refinement on offspring (increased due to parallelization)
    private readonly int offspringLocalSearchIterations = 20;
    // Probability of applying local search to offspring (0.0-1.0)
    private readonly double offspringLocalSearchProbability = 0.40;
    // Generations without improvement before early termination (0 = disabled)
    private readonly int earlyTerminationGenerations = 12;

    // Returns more conservative effective sizes for huge datasets since LS adds overhead
    // Respects user hyperparameters by applying scaling ratios instead of hardcoding
    protected override (int populationSize, int generations) GetEffectiveSizes(int taskCount)
    {
        int userPopulation = parameters.PopulationSize;
        int userGenerations = parameters.Generations;
        
        // For huge datasets, apply scaling to user's configured values
        if (taskCount > 500)
        {
            // Reduce to 60% of user's config for very large problems
            int scaledPopulation = Math.Max(userPopulation * 60 / 100, 10);
            int scaledGenerations = Math.Max(userGenerations * 45 / 100, 15);
            return (scaledPopulation, scaledGenerations);
        }
        
        if (taskCount > 250)
        {
            // Reduce to 75% of user's config for large problems
            int scaledPopulation = Math.Max(userPopulation * 75 / 100, 12);
            int scaledGenerations = Math.Max(userGenerations * 65 / 100, 20);
            return (scaledPopulation, scaledGenerations);
        }
        
        if (taskCount > 120)
        {
            // Reduce to 85% of user's config for medium-large problems
            int scaledPopulation = Math.Max(userPopulation * 85 / 100, 14);
            int scaledGenerations = Math.Max(userGenerations * 85 / 100, 30);
            return (scaledPopulation, scaledGenerations);
        }
        
        // For smaller datasets, use user's exact configuration
        return (userPopulation, userGenerations);
    }

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

    // Executes generations with local search refinement and early termination
    private void EvolveGenerationsWithLocalSearch(EvolutionState state)
    {
        int generationsWithoutImprovement = 0;
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
                generationsWithoutImprovement = 0;
            }
            else
            {
                generationsWithoutImprovement++;
            }

            // Early termination if no improvement for N generations
            if (earlyTerminationGenerations > 0 && generationsWithoutImprovement >= earlyTerminationGenerations)
            {
                break;
            }

            var nextPopulation = new List<List<JSPTask>>();

            // Apply local search only to the single best elite individual
            var bestEliteSequence = new List<JSPTask>(scoredPopulation[0].Sequence);
            int evals = state.Evaluations;
            int lsApps = state.LocalSearchApplications;
            var improvedBest = ApplyLocalSearch(bestEliteSequence, state.PredecessorMap, ref evals, ref lsApps, localSearchIterations);
            state.Evaluations = evals;
            state.LocalSearchApplications = lsApps;
            nextPopulation.Add(improvedBest);
            
            // Add remaining elite without local search
            for (int eliteIndex = 1; eliteIndex < parameters.EliteCount; eliteIndex++)
            {
                nextPopulation.Add([.. scoredPopulation[eliteIndex].Sequence]);
            }

            // Generate offspring in parallel
            int offspringNeeded = state.EffectivePopulationSize - nextPopulation.Count;
            var offspringBag = new ConcurrentBag<List<JSPTask>>();
            int localEvaluations = 0;
            int localLSApplications = 0;
            object counterLock = new();

            Parallel.For(0, offspringNeeded, () => (evals: 0, lsApps: 0), (i, loop, threadState) =>
            {
                var parentA = TournamentSelect(scoredPopulation);
                var parentB = TournamentSelect(scoredPopulation);

                var child = crossoverOperator.Crossover(parentA, parentB);

                if (Random.Shared.NextDouble() < parameters.MutationRate)
                {
                    mutationOperator.Mutate(child);
                }

                var repairedChild = SequenceRepair.RepairToFeasibleOrder(child, state.PredecessorMap);
                int localEvals = threadState.evals;
                int localLS = threadState.lsApps;
                
                // Apply local search to offspring probabilistically with reduced iterations
                List<JSPTask> finalChild = repairedChild;
                if (Random.Shared.NextDouble() < offspringLocalSearchProbability)
                {
                    finalChild = ApplyLocalSearch(
                        repairedChild, 
                        state.PredecessorMap, 
                        ref localEvals, 
                        ref localLS,
                        offspringLocalSearchIterations);
                }
                
                offspringBag.Add(finalChild);
                return (localEvals, localLS);
            }, threadState =>
            {
                lock (counterLock)
                {
                    localEvaluations += threadState.evals;
                    localLSApplications += threadState.lsApps;
                }
            });

            state.Evaluations += localEvaluations;
            state.LocalSearchApplications += localLSApplications;
            nextPopulation.AddRange(offspringBag);

            state.Population.Clear();
            state.Population.AddRange(nextPopulation);
        }
    }

    // Applies hill-climbing local search to improve a candidate solution
    private List<JSPTask> ApplyLocalSearch(
        List<JSPTask> sequence,
        IReadOnlyDictionary<string, string?> predecessorByTaskKey,
        ref int evaluations,
        ref int localSearchApplications,
        int? maxIterations = null)
    {
        int maxIts = maxIterations ?? localSearchIterations;
        List<JSPTask> current = [.. sequence];
        int currentMakespan = ScheduleEvaluation.EvaluateMakespan(current, predecessorByTaskKey);
        evaluations++;

        // Run hill-climbing iterations until no improvement is found
        int iterations = 0;
        while (iterations < maxIts)
        {
            iterations++;
            bool foundImprovement = false;

            // Try every adjacent swap until an improvement is found
            foreach ((_, List<JSPTask> candidate) in LocalSearchNeighbourhood.GenerateAdjacentSwapCandidates(current))
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
                // Stop when the neighbourhood contains no better solution
                break;
            }
        }

        localSearchApplications++;
        return current;
    }
}
