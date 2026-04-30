namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Factories;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Factory interface for creating scheduling algorithm instances
public interface IAlgorithmFactory
{
    ISchedulingAlgorithm Create(AlgorithmId algorithmId);
}
