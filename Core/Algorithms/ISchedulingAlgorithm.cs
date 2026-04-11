public interface ISchedulingAlgorithm
{
    AlgorithmCategory Category { get; }
    AlgorithmId Id { get; }
    string DisplayName { get; }
    AlgorithmExecutionResult Execute(Schedule schedule);
}