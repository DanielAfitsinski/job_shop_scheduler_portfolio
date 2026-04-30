namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Parameters for Tabu Search algorithm with tabu tenure and termination strategy
public class TabuSearchParameters : ITabuSearchParameters
{
    public required string ConfigurationName { get; init; }
    public int MaxIterations { get; init; }
    public int MultiStartSeeds { get; init; }
    public int TabuTenure { get; init; }
    
    // Early termination: stop if no improvement found for this many iterations
    public int MaxIterationsWithoutImprovement { get; init; }

    // Creates Tabu Search parameters with default values
    public TabuSearchParameters()
    {
        ConfigurationName = "Default";
        MaxIterations = 500;
        MultiStartSeeds = 5;
        TabuTenure = 7;
        MaxIterationsWithoutImprovement = 50; // Stop if no improvement for 50 iterations
    }

    public string? Validate()
    {
        // Helper to check conditions
        static string? CheckCondition(bool isInvalid, string message) =>
            isInvalid ? message : null;

        // Base local search validation
        return
            CheckCondition(MaxIterations <= 0, "MaxIterations must be greater than 0") ??
            CheckCondition(MultiStartSeeds <= 0, "MultiStartSeeds must be greater than 0") ??
            CheckCondition(MultiStartSeeds > 100, "MultiStartSeeds should not exceed 100") ??
            CheckCondition(TabuTenure <= 0, "TabuTenure must be greater than 0") ??
            CheckCondition(TabuTenure > MaxIterations / 2, "TabuTenure should not exceed half of MaxIterations") ??
            CheckCondition(MaxIterationsWithoutImprovement < 0, "MaxIterationsWithoutImprovement must be 0 or greater");
    }
}
