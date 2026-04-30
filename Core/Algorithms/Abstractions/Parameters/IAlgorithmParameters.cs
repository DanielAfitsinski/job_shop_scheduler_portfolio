namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Base interface for all algorithm parameters
// Enables parameter configuration across different algorithm families
public interface IAlgorithmParameters
{
    string ConfigurationName { get; }

    string? Validate();
}
