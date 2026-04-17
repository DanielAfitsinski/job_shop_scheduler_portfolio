namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;

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
    // Executes the algorithm for the selected schedule
    AlgorithmExecutionResult Execute(Schedule schedule);
}