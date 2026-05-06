namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Implementations;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Genetic algorithm
public class GeneticAlgorithm : EvolutionaryAlgorithm
{
    // Crossover strategy used by this genetic algorithm
    private ICrossoverOperator crossoverOperator;
    // Mutation strategy used by this genetic algorithm
    private IMutationOperator mutationOperator;

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

            for (int eliteIndex = 0; eliteIndex < parameters.EliteCount; eliteIndex++)
            {
                nextPopulation.Add([.. scoredPopulation[eliteIndex].Sequence]);
            }

            while (nextPopulation.Count < state.EffectivePopulationSize)
            {
                var parentA = TournamentSelect(scoredPopulation);
                var parentB = TournamentSelect(scoredPopulation);

                var child = crossoverOperator.Crossover(parentA, parentB);

                if (Random.Shared.NextDouble() < parameters.MutationRate)
                {
                    mutationOperator.Mutate(child);
                }

                var repairedChild = RepairToFeasibleOrder(child, state.PredecessorMap);
                nextPopulation.Add(repairedChild);
            }

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

    // Repairs a candidate order so every task appears after its predecessor
    protected static new List<JSPTask> RepairToFeasibleOrder(
        IReadOnlyList<JSPTask> proposed,
        IReadOnlyDictionary<string, string?> predecessorByTaskKey)
    {
        // Copy the candidate so tasks can be removed as they become schedulable
        List<JSPTask> pending = [.. proposed];
        // Build the repaired order incrementally
        List<JSPTask> repaired = [];
        // Track which tasks have already been completed
        HashSet<string> completed = [];

        while (pending.Count > 0)
        {
            bool progressed = false;

            // Walk the remaining tasks and emit any task whose predecessor is done
            for (int index = 0; index < pending.Count; index++)
            {
                JSPTask task = pending[index];
                string taskKey = CreateTaskKey(task);
                string? predecessor = predecessorByTaskKey[taskKey];

                if (predecessor is not null && !completed.Contains(predecessor))
                {
                    // Keep waiting until the predecessor has been scheduled
                    continue;
                }

                // Add the task to the repaired output and mark it complete
                repaired.Add(task);
                completed.Add(taskKey);
                pending.RemoveAt(index);
                progressed = true;
                index--;
            }

            if (!progressed)
            {
                // Defensive fallback for malformed inputs
                repaired.AddRange(pending);
                break;
            }
        }

        // Return the feasibility-repaired chromosome
        return repaired;
    }

    // Creates a stable key for a task so duplicates can be tracked
    protected static string CreateTaskKey(JSPTask task)
    {
        return $"{task.JobId}:{task.Operation}";
    }
}
