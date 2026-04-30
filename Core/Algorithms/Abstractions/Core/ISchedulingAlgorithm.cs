namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Shared contract for all scheduling algorithms
public interface ISchedulingAlgorithm
{
    // The family this algorithm belongs to
    AlgorithmCategory Category { get; }
    // The Identifier used by the UI
    AlgorithmId Id { get; }
    // The algorithm display name
    string DisplayName { get; }

    // Gets the current parameters for this algorithm
    IAlgorithmParameters Parameters { get; }

    // Configures the algorithm with new parameters
    void ConfigureParameters(IAlgorithmParameters parameters);

    // Executes the algorithm for the selected schedule using current parameters
    AlgorithmExecutionResult Execute(Schedule schedule);
}
