namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Parameters specific to Tabu Search algorithm
public interface ITabuSearchParameters : ILocalSearchParameters
{
    // Length of the tabu list for forbidden moves
    int TabuTenure { get; }

    // Terminate early if no improvement found for this many iterations (0 = no early termination)
    int MaxIterationsWithoutImprovement { get; }
}

// Defines different strategies for generating neighborhoods in Tabu Search
public enum TabuNeighborhoodStrategy
{
    Dynamic = 0
}
