namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Factories;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Describes an available algorithm and can create instances of it
public interface IAlgorithmDescriptor
{
    // The algorithm ID
    AlgorithmId Id { get; }
    // The display name
    string DisplayName { get; }
    // Creates an instance of the algorithm
    ISchedulingAlgorithm Create();
}
