namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;

// Describes an available mutation operator and can create instances of it
public interface IMutationOperatorDescriptor
{
    // User-friendly display name for menu selection
    string DisplayName { get; }

    // Creates a new instance of the operator
    IMutationOperator Create();
}
