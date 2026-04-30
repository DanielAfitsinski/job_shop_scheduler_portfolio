namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Services;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Implementations;

// Factory for creating crossover operators
public class CrossoverOperatorFactory
{
    private static readonly ICrossoverOperatorDescriptor[] AvailableOperators =
    [
        new CrossoverOperatorDescriptor("Ordered Crossover (OX)", () => new OrderedCrossoverOperator())
    ];

    // Returns all available crossover operators
    public static IReadOnlyList<ICrossoverOperatorDescriptor> GetAvailableOperators() 
        => AvailableOperators;

    // Creates an operator by index
    public static ICrossoverOperator? CreateByIndex(int index)
    {
        if (index < 0 || index >= AvailableOperators.Length)
            return null;
        
        return AvailableOperators[index].Create();
    }

    // Descriptor implementation for crossover operators
    private class CrossoverOperatorDescriptor(string displayName, Func<ICrossoverOperator> factory) 
        : ICrossoverOperatorDescriptor
    {
        public string DisplayName { get; } = displayName;

        public ICrossoverOperator Create() => factory();
    }
}
