namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Strategy pattern for genetic mutation operators
public interface IMutationOperator
{
    // Name of the mutation strategy
    string Name { get; }

    // Applies mutation to a chromosome
    void Mutate(List<JSPTask> chromosome);
}
