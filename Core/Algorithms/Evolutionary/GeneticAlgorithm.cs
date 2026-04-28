namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Genetic algorithm that evolves task sequences toward lower makespan
public class GeneticAlgorithm(
    int populationSize = 30,
    int generations = 80,
    double mutationRate = 0.20,
    int eliteCount = 2,
    int tournamentSize = 3) : EvolutionaryAlgorithm(populationSize, generations, mutationRate, eliteCount, tournamentSize)
{
    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.GeneticAlgorithm;
    // Algorithm display name
    public override string DisplayName => "Genetic Algorithm";

    // Returns effective population and generation sizes for large schedules
    protected override (int populationSize, int generations) GetEffectiveSizes(int taskCount)
    {
        int effectivePopulation = taskCount > 120 ? Math.Min(populationSize, 20) : populationSize;
        int effectiveGenerations = taskCount > 120 ? Math.Min(generations, 45) : generations;
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

            for (int eliteIndex = 0; eliteIndex < eliteCount; eliteIndex++)
            {
                nextPopulation.Add([.. scoredPopulation[eliteIndex].Sequence]);
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
                nextPopulation.Add(repairedChild);
            }

            state.Population.Clear();
            state.Population.AddRange(nextPopulation);
        }
    }

    // Builds the result message summarising the evolutionary search
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
            $"Evaluations: {state.Evaluations}\n" +
            $"Elapsed: {state.Stopwatch.ElapsedMilliseconds} ms";

        return new AlgorithmExecutionResult(
            "Genetic Algorithm Result",
            message,
            computedSchedule: state.BestSequence,
            makespan: state.BestMakespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }

    // Performs ordered crossover
    protected static List<JSPTask> Crossover(IReadOnlyList<JSPTask> parentA, IReadOnlyList<JSPTask> parentB)
    {
        int length = parentA.Count;
        int start = Random.Shared.Next(length);
        int end = Random.Shared.Next(start, length);

        // Fill the child with the preserved slice and then backfill from the second parent
        JSPTask?[] child = new JSPTask?[length];
        HashSet<string> inherited = [];

        for (int index = start; index <= end; index++)
        {
            JSPTask task = parentA[index];
            child[index] = task;
            inherited.Add(CreateTaskKey(task));
        }

        int insertIndex = 0;
        foreach (JSPTask task in parentB)
        {
            string key = CreateTaskKey(task);
            if (inherited.Contains(key))
            {
                // Skip tasks that already came from the preserved slice
                continue;
            }

            while (insertIndex < length && child[insertIndex] is not null)
            {
                // Find the next empty child slot
                insertIndex++;
            }

            if (insertIndex < length)
            {
                // Place the remaining tasks in parentB order
                child[insertIndex] = task;
            }
        }

        // Return the fully assembled child sequence
        return [.. child.Where(task => task is not null).Select(task => task!)];
    }

    // Applies a simple swap mutation to a chromosome
    protected static new void Mutate(List<JSPTask> chromosome)
    {
        if (chromosome.Count < 2)
        {
            // Nothing to mutate when there are fewer than two tasks
            return;
        }

        // Swap two randomly chosen positions
        int firstIndex = Random.Shared.Next(chromosome.Count);
        int secondIndex = Random.Shared.Next(chromosome.Count);
        while (secondIndex == firstIndex)
        {
            secondIndex = Random.Shared.Next(chromosome.Count);
        }

        (chromosome[firstIndex], chromosome[secondIndex]) = (chromosome[secondIndex], chromosome[firstIndex]);
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

    // Shuffles the seed sequence using Fisher-Yates
    protected static void Shuffle(List<JSPTask> items)
    {
        for (int index = items.Count - 1; index > 0; index--)
        {
            // Swap the current item with a random earlier item
            int swapIndex = Random.Shared.Next(index + 1);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }
    }

    // Creates a stable key for a task so duplicates can be tracked
    protected static string CreateTaskKey(JSPTask task)
    {
        return $"{task.JobId}:{task.Operation}";
    }
}
