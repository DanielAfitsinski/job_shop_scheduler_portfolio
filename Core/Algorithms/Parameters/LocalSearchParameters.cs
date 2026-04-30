namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Parameters for local search algorithms with multi-start capability
public class LocalSearchParameters : ILocalSearchParameters
{
    public required string ConfigurationName { get; init; }
    public int MaxIterations { get; init; }
    public int MultiStartSeeds { get; init; }

    // Creates local search parameters with default values
    public LocalSearchParameters()
    {
        ConfigurationName = "Default";
        MaxIterations = 1000;
        MultiStartSeeds = 5;
    }

    public string? Validate()
    {
        // Helper to check a condition
        static string? CheckCondition(bool isInvalid, string message) =>
            isInvalid ? message : null;

        return
            CheckCondition(MaxIterations <= 0, "MaxIterations must be greater than 0") ??
            CheckCondition(MultiStartSeeds <= 0, "MultiStartSeeds must be greater than 0") ??
            CheckCondition(MultiStartSeeds > 100, "MultiStartSeeds should not exceed 100 for performance reasons") ??
            CheckCondition(MaxIterations > 100000, "MaxIterations should not exceed 10,000");
    }
}
