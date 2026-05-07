namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;

using System.Collections.Concurrent;
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
            EffectiveGenerations = effectiveGens
        };
    }

    // Subclasses return effective population and generation sizes based on task count
    protected abstract (int populationSize, int generations) GetEffectiveSizes(int taskCount);

    // Subclasses implement their specific evolution loop
    protected abstract void EvolvePopulation(EvolutionState state);

    // Subclasses implement their specific result message
    protected abstract AlgorithmExecutionResult BuildResultMessage(Schedule schedule, EvolutionState state);

    // Builds the initial population from the SPT seed and randomised variants in parallel
    protected static List<List<JSPTask>> BuildInitialPopulation(
        IReadOnlyList<JSPTask> seed,
        IReadOnlyDictionary<string, string?> predecessorByTaskKey,
        int populationSize)
    {
        // Use a thread-safe collection for building population in parallel
        var population = new ConcurrentBag<List<JSPTask>>
        {
            // Add the seed as the first individual
            ([.. seed])
        };

        // Create additional diverse individuals by randomising the seed
        // Diversity in the initial population improves search exploration
        int remainingIndividuals = populationSize - 1;
        var tasks = new List<Task>();
        
        // Generate each additional individual in parallel for efficiency
        for (int i = 0; i < remainingIndividuals; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                // Shuffle the seed randomly to create variation
                List<JSPTask> randomised = [.. seed.OrderBy(_ => Random.Shared.Next())];
                // Repair the randomised sequence to ensure job precedence constraints are satisfied
                List<JSPTask> repaired = SequenceRepair.RepairToFeasibleOrder(randomised, predecessorByTaskKey);
                population.Add(repaired);
            }));
        }
        
        // Wait for all parallel tasks to complete
        Task.WaitAll([.. tasks]);
        // Extract the population and ensure it's exactly the requested size
        return [.. population.Take(populationSize)];
    }

    // Record for a population member with its makespan
    protected record ScoredIndividual(List<JSPTask> Sequence, int Makespan);

    // Scores the entire population in parallel and returns sorted by makespan (best first)
    protected static List<ScoredIndividual> ScorePopulation(
        IReadOnlyList<List<JSPTask>> population,
        IReadOnlyDictionary<string, string?> predecessorMap,
        ref int evaluations)
    {
        // Use thread-safe collection for parallel scoring results
        var scored = new ConcurrentBag<ScoredIndividual>();
        int localEvaluations = 0;
        // Lock ensures thread-safe evaluation counter updates
        object evaluationLock = new();

        // Evaluate all individuals in parallel for performance
        Parallel.ForEach(population, individual =>
        {
            // Compute the makespan for this individual
            int makespan = ScheduleEvaluation.EvaluateMakespan(individual, predecessorMap);
            scored.Add(new ScoredIndividual(individual, makespan));
            
            // Thread-safely increment the evaluation counter
            lock (evaluationLock)
            {
                localEvaluations++;
            }
        });

        // Update the total evaluations count
        evaluations += localEvaluations;
        // Return population sorted by fitness (lowest makespan first = best solutions first)
        return [.. scored.OrderBy(s => s.Makespan)];
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
