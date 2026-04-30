namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Parameters for construction heuristic algorithms (SPT, LPT & Random)
public interface IHeuristicParameters : IAlgorithmParameters
{
    // Construction heuristics have no runtime parameters
    // This interface exists for future extensibility and consistent design
}
