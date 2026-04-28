namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Factory responsible for creating algorithm instances
public class AlgorithmFactory : IAlgorithmFactory
{
    // Creates an algorithm instance based on the requested algorithm ID
    public ISchedulingAlgorithm Create(AlgorithmId algorithmId)
    {
        return algorithmId switch
        {
            AlgorithmId.ShortestProcessingTime => new ShortestProcessingTimeAlgorithm(),
            AlgorithmId.LongestProcessingTime => new LongestProcessingTimeAlgorithm(),
            AlgorithmId.RandomHeuristic => new RandomAlgorithm(),
            AlgorithmId.HillClimbing => new HillClimbingAlgorithm(),
            AlgorithmId.TabuSearch => new TabuSearchAlgorithm(
                tabuTenure: 7,
                maxIterations: 500,
                useDoubleNeighborhoods: false),
            AlgorithmId.GeneticAlgorithm => new GeneticAlgorithm(),
            AlgorithmId.MemeticHybrid => new MemeticHybridAlgorithm(),
            _ => throw new ArgumentException($"Unknown algorithm: {algorithmId}", nameof(algorithmId))
        };
    }
}
