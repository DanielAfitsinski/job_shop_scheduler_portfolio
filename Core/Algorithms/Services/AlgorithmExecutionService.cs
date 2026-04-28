namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Executes scheduling algorithms using the factory pattern
public class AlgorithmExecutionService(IAlgorithmFactory factory)
{
    private readonly IAlgorithmFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    // Executes the algorithm requested by the menu
    public AlgorithmExecutionResult Execute(Schedule schedule, AlgorithmId algorithmId)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        try
        {
            ISchedulingAlgorithm algorithm = _factory.Create(algorithmId);
            return algorithm.Execute(schedule);
        }
        catch (ArgumentException ex)
        {
            return new AlgorithmExecutionResult(
                "Algorithm Not Implemented",
                $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\nAlgorithm: {algorithmId}\n\n{ex.Message}",
                isError: true);
        }
    }
}