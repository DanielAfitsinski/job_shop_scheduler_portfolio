namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Default parameters for construction heuristic algorithms
public class HeuristicParameters : IHeuristicParameters
{
    public string ConfigurationName { get; init; } = "Default";

    public string? Validate()
    {
        // Heuristics have no parameters to validate
        return null;
    }
}
