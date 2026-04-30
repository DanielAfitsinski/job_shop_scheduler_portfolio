namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;

using System.Diagnostics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Base class for evolutionary algorithms
public abstract class EvolutionaryAlgorithm : ISchedulingAlgorithm
{
    // Stores the current parameters for this algorithm
    protected IEvolutionaryParameters parameters;

    // All evolutionary algorithms belong to this category
    public AlgorithmCategory Category => AlgorithmCategory.Evolutionary;

    // Subclasses define their specific algorithm identifier
    public abstract AlgorithmId Id { get; }

    // Subclasses define their display name
    public abstract string DisplayName { get; }

    // Gets the current parameters for this algorithm
    public IAlgorithmParameters Parameters => parameters;

    // Configures the algorithm with new evolutionary parameters
    public void ConfigureParameters(IAlgorithmParameters newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);

        if (newParameters is not IEvolutionaryParameters evolutionaryParams)
        {
            throw new ArgumentException(
                $"Parameters must be of type {nameof(IEvolutionaryParameters)}, got {newParameters.GetType().Name}",
                nameof(newParameters));
        }

        string? validationError = newParameters.Validate();
        if (validationError is not null)
        {
            throw new ArgumentException(validationError, nameof(newParameters));
        }

        parameters = evolutionaryParams;
    }

    // Constructor with default parameters
    protected EvolutionaryAlgorithm()
    {
        parameters = new EvolutionaryParameters { ConfigurationName = "Default" };
    }

    // Common Execute pattern for all evolutionary algorithms
    public AlgorithmExecutionResult Execute(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        if (schedule.tasks.Length == 0)
        {
            return new AlgorithmExecutionResult("No Tasks", "The selected schedule has no tasks.", isError: true);
        }

        var state = InitialiseEvolution(schedule);
        EvolvePopulation(state);
        state.Stopwatch.Stop();
        return BuildResultMessage(schedule, state);
    }

    // Base state class for evolutionary search
    protected class EvolutionState
    {
        public List<List<JSPTask>> Population { get; set; } = [];
        public Dictionary<string, string?> PredecessorMap { get; set; } = [];
        public int InitialMakespan { get; set; }
        public int BestMakespan { get; set; }
        public List<JSPTask> BestSequence { get; set; } = [];
        public int Evaluations { get; set; }
        public int LocalSearchApplications { get; set; }
        public int TaskCount { get; set; }
        public int EffectivePopulationSize { get; set; }
        public int EffectiveGenerations { get; set; }
        public Stopwatch Stopwatch { get; set; } = new();
    }

    // Initialises the evolutionary search with seed population and state
    private EvolutionState InitialiseEvolution(Schedule schedule)
    {
        int taskCount = schedule.tasks.Length;
        (int effectivePopulation, int effectiveGens) = GetEffectiveSizes(taskCount);

        Dictionary<string, string?> predecessorMap = ScheduleEvaluation.BuildPredecessorMap(schedule.tasks);
        var sptAlgorithm = new ShortestProcessingTimeAlgorithm();
        List<JSPTask> seed = [.. sptAlgorithm.BuildSequence(schedule)];
        int initialMakespan = ScheduleEvaluation.EvaluateMakespan(seed, predecessorMap);

        List<List<JSPTask>> population = BuildInitialPopulation(seed, predecessorMap, effectivePopulation);

        return new EvolutionState
        {
            Population = population,
            PredecessorMap = predecessorMap,
            InitialMakespan = initialMakespan,
            BestMakespan = int.MaxValue,
            BestSequence = [.. seed],
            Evaluations = 0,
            TaskCount = taskCount,
            EffectivePopulationSize = effectivePopulation,
            EffectiveGenerations = effectiveGens,
            Stopwatch = Stopwatch.StartNew()
        };
    }

    // Subclasses return effective population and generation sizes based on task count
    protected abstract (int populationSize, int generations) GetEffectiveSizes(int taskCount);

    // Subclasses implement their specific evolution loop
    protected abstract void EvolvePopulation(EvolutionState state);

    // Subclasses implement their specific result message
    protected abstract AlgorithmExecutionResult BuildResultMessage(Schedule schedule, EvolutionState state);

    // Builds the initial population from the SPT seed and randomised variants
    protected static List<List<JSPTask>> BuildInitialPopulation(
        IReadOnlyList<JSPTask> seed,
        IReadOnlyDictionary<string, string?> predecessorByTaskKey,
        int populationSize)
    {
        // Include the seed as the first individual
        List<List<JSPTask>> population = [[.. seed]];

        // Add randomised, repaired copies until the population is full
        while (population.Count < populationSize)
        {
            List<JSPTask> randomised = [.. seed.OrderBy(_ => Random.Shared.Next())];
            List<JSPTask> repaired = RepairToFeasibleOrder(randomised, predecessorByTaskKey);
            population.Add(repaired);
        }

        return population;
    }

    // Record for a population member with its makespan
    protected record ScoredIndividual(List<JSPTask> Sequence, int Makespan);

    // Scores the entire population and returns sorted by makespan (best first)
    protected static List<ScoredIndividual> ScorePopulation(
        IReadOnlyList<List<JSPTask>> population,
        IReadOnlyDictionary<string, string?> predecessorMap,
        ref int evaluations)
    {
        var scored = new List<ScoredIndividual>();
        foreach (var individual in population)
        {
            int makespan = ScheduleEvaluation.EvaluateMakespan(individual, predecessorMap);
            scored.Add(new ScoredIndividual(individual, makespan));
            evaluations++;
        }

        return [.. scored.OrderBy(s => s.Makespan)];
    }

    // Applies ordered crossover between two parents
    protected static List<JSPTask> Crossover(List<JSPTask> parentA, List<JSPTask> parentB)
    {
        int size = parentA.Count;
        int start = Random.Shared.Next(size);
        int end = Random.Shared.Next(start, size);

        var child = new List<JSPTask>(size);
        var used = new HashSet<string>();

        // Copy segment from parent A
        for (int i = start; i <= end; i++)
        {
            child.Add(parentA[i]);
            used.Add($"{parentA[i].JobId}:{parentA[i].Operation}");
        }

        // Fill remaining positions from parent B (in order)
        int childPos = (end + 1) % size;
        for (int i = 0; i < size; i++)
        {
            string key = $"{parentB[i].JobId}:{parentB[i].Operation}";
            if (!used.Contains(key))
            {
                if (childPos == start)
                {
                    childPos = end + 1;
                }
                if (child.Count < size)
                {
                    child.Add(parentB[i]);
                    childPos = (childPos + 1) % size;
                }
            }
        }

        return child;
    }

    // Applies mutation by swapping random positions
    protected static void Mutate(List<JSPTask> individual)
    {
        if (individual.Count < 2)
        {
            return;
        }

        int pos1 = Random.Shared.Next(individual.Count);
        int pos2 = Random.Shared.Next(individual.Count);

        (individual[pos1], individual[pos2]) = (individual[pos2], individual[pos1]);
    }

    // Repairs a sequence to satisfy job precedence constraints
    protected static List<JSPTask> RepairToFeasibleOrder(
        IReadOnlyList<JSPTask> sequence,
        IReadOnlyDictionary<string, string?> predecessorMap)
    {
        Dictionary<string, string?> pred = new(predecessorMap);
        List<JSPTask> repaired = [];
        HashSet<string> completed = [];
        Queue<JSPTask> pending = new(sequence);

        while (pending.Count > 0 || repaired.Count < sequence.Count)
        {
            var next = pending.Dequeue();
            string taskKey = $"{next.JobId}:{next.Operation}";
            string? predKey = pred.TryGetValue(taskKey, out string? p) ? p : null;

            if (predKey is not null && !completed.Contains(predKey))
            {
                pending.Enqueue(next);
            }
            else
            {
                repaired.Add(next);
                completed.Add(taskKey);
            }
        }

        return repaired;
    }

    // Tournament selection: randomly select configured number of individuals and return the best sequence
    protected List<JSPTask> TournamentSelect(List<ScoredIndividual> population)
    {
        ScoredIndividual best = population[Random.Shared.Next(population.Count)];

        for (int i = 1; i < parameters.TournamentSize; i++)
        {
            var candidate = population[Random.Shared.Next(population.Count)];
            if (candidate.Makespan < best.Makespan)
            {
                best = candidate;
            }
        }

        return best.Sequence;
    }
}
