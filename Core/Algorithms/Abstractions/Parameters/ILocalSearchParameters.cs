namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Parameters for local search algorithms (Hill Climbing & Tabu Search)
public interface ILocalSearchParameters : IAlgorithmParameters
{
    // Maximum number of iterations without improvement before terminating
    int MaxIterations { get; }

    // Number of independent search runs starting from different seeds
    int MultiStartSeeds { get; }
}
