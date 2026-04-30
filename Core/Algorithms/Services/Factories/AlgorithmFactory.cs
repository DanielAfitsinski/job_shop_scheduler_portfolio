namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services.Factories;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Factories;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Factory responsible for creating algorithm instances
public class AlgorithmFactory : IAlgorithmFactory
{
    private static readonly IAlgorithmDescriptor[] AvailableAlgorithms =
    [
        new AlgorithmDescriptor(AlgorithmId.TabuSearch, "Tabu Search", () => new TabuSearchAlgorithm()),
        new AlgorithmDescriptor(AlgorithmId.GeneticAlgorithm, "Genetic Algorithm", () => new GeneticAlgorithm()),
        new AlgorithmDescriptor(AlgorithmId.MemeticHybrid, "Memetic Hybrid", () => new MemeticHybridAlgorithm())
    ];

    // Returns all available algorithms
    public static IReadOnlyList<IAlgorithmDescriptor> GetAvailableAlgorithms() 
        => AvailableAlgorithms;

    // Creates an algorithm instance based on the requested algorithm ID
    public ISchedulingAlgorithm Create(AlgorithmId algorithmId)
    {
        return algorithmId switch
        {
            AlgorithmId.ShortestProcessingTime => new ShortestProcessingTimeAlgorithm(),
            AlgorithmId.LongestProcessingTime => new LongestProcessingTimeAlgorithm(),
            AlgorithmId.RandomHeuristic => new RandomAlgorithm(),
            AlgorithmId.HillClimbing => new HillClimbingAlgorithm(),
            AlgorithmId.TabuSearch => new TabuSearchAlgorithm(),
            AlgorithmId.GeneticAlgorithm => new GeneticAlgorithm(),
            AlgorithmId.MemeticHybrid => new MemeticHybridAlgorithm(),
            _ => throw new ArgumentException($"Unknown algorithm: {algorithmId}", nameof(algorithmId))
        };
    }

    // Descriptor implementation for algorithms
    private record AlgorithmDescriptor(AlgorithmId Id, string DisplayName, Func<ISchedulingAlgorithm> Factory)
        : IAlgorithmDescriptor
    {
        public ISchedulingAlgorithm Create() => Factory();
    }
}
