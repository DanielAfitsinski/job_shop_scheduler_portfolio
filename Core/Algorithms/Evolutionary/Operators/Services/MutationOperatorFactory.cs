namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Services;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Implementations;

// Factory for creating mutation operators
public class MutationOperatorFactory
{
    private static readonly IMutationOperatorDescriptor[] AvailableOperators =
    [
        new MutationOperatorDescriptor("Simple Swap", () => new SimpleSwapMutationOperator())
    ];

    // Returns all available mutation operators
    public static IReadOnlyList<IMutationOperatorDescriptor> GetAvailableOperators() 
        => AvailableOperators;

    // Creates an operator by index
    public static IMutationOperator? CreateByIndex(int index)
    {
        if (index < 0 || index >= AvailableOperators.Length)
            return null;
        
        return AvailableOperators[index].Create();
    }

    // Descriptor implementation for mutation operators
    private class MutationOperatorDescriptor(string displayName, Func<IMutationOperator> factory) 
        : IMutationOperatorDescriptor
    {
        public string DisplayName { get; } = displayName;

        public IMutationOperator Create() => factory();
    }
}
