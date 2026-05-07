namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;

using System.Collections.Concurrent;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Implementations;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Genetic algorithm
public class GeneticAlgorithm : EvolutionaryAlgorithm
{
    // Crossover strategy used by this genetic algorithm
    protected ICrossoverOperator crossoverOperator;
    // Mutation strategy used by this genetic algorithm
    protected IMutationOperator mutationOperator;

    // Initialises genetic algorithm with default operators
    public GeneticAlgorithm()
    {
        crossoverOperator = new OrderedCrossoverOperator();
        mutationOperator = new SimpleSwapMutationOperator();
    }

    // Allows configuration of custom genetic operators
    public void SetCrossoverOperator(ICrossoverOperator crossoverOp)
    {
        crossoverOperator = crossoverOp ?? throw new ArgumentNullException(nameof(crossoverOp));
    }

    // Allows configuration of custom genetic operators
    public void SetMutationOperator(IMutationOperator mutationOp)
    {
        mutationOperator = mutationOp ?? throw new ArgumentNullException(nameof(mutationOp));
    }

    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.GeneticAlgorithm;
    // Algorithm display name
    public override string DisplayName => "Genetic Algorithm";

    // Returns effective population and generation sizes for large schedules
    protected override (int populationSize, int generations) GetEffectiveSizes(int taskCount)
    {
        int effectivePopulation = taskCount > 120 ? Math.Min(parameters.PopulationSize, 20) : parameters.PopulationSize;
        int effectiveGenerations = taskCount > 120 ? Math.Min(parameters.Generations, 45) : parameters.Generations;
        return (effectivePopulation, effectiveGenerations);
    }

    // Executes the main evolutionary loop over all generations
    protected override void EvolvePopulation(EvolutionState state)
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

            // Add elite individuals to next population
            for (int eliteIndex = 0; eliteIndex < parameters.EliteCount; eliteIndex++)
            {
                nextPopulation.Add([.. scoredPopulation[eliteIndex].Sequence]);
            }

            // Generate offspring in parallel for better performance
            int offspringNeeded = state.EffectivePopulationSize - nextPopulation.Count;
            var offspringBag = new ConcurrentBag<List<JSPTask>>();

            Parallel.For(0, offspringNeeded, i =>
            {
                // Tournament selection to choose parents
                var parentA = TournamentSelect(scoredPopulation);
                var parentB = TournamentSelect(scoredPopulation);

                // Crossover to create offspring
                var child = crossoverOperator.Crossover(parentA, parentB);

                // Mutation with configured probability
                if (Random.Shared.NextDouble() < parameters.MutationRate)
                {
                    mutationOperator.Mutate(child);
                }

                // Repair to maintain job precedence constraints
                var repairedChild = SequenceRepair.RepairToFeasibleOrder(child, state.PredecessorMap);
                offspringBag.Add(repairedChild);
            });

            // Combine elite and offspring populations
            nextPopulation.AddRange(offspringBag);

            state.Population.Clear();
            state.Population.AddRange(nextPopulation);
        }
    }

    // Builds the result message summarising the evolutionary search
    protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, EvolutionState state)
    {
        string message = AlgorithmResultFormatter.BuildStandardMessage(
            schedule,
            DisplayName,
            state.TaskCount,
            state.BestMakespan);

        return new AlgorithmExecutionResult(
            "Genetic Algorithm Result",
            message,
            computedSchedule: state.BestSequence,
            makespan: state.BestMakespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }
}
