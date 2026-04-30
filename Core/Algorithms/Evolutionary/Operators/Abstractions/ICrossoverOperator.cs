namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Strategy pattern for genetic crossover operators
public interface ICrossoverOperator
{
    // Name of the crossover strategy
    string Name { get; }

    // Performs crossover on two parent sequences to produce offspring
    List<JSPTask> Crossover(IReadOnlyList<JSPTask> parentA, IReadOnlyList<JSPTask> parentB);
}
