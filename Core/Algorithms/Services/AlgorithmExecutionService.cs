namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Factories;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Executes scheduling algorithms using the factory pattern
public class AlgorithmExecutionService(IAlgorithmFactory factory)
{
    private readonly IAlgorithmFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    // Executes the algorithm requested by the menu with default parameters
    public AlgorithmExecutionResult Execute(Schedule schedule, AlgorithmId algorithmId)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            ISchedulingAlgorithm algorithm = _factory.Create(algorithmId);
            AlgorithmExecutionResult result = algorithm.Execute(schedule);
            stopwatch.Stop();
            
            result.ExecutionMilliseconds = (int)stopwatch.ElapsedMilliseconds;
            return result;
        }
        catch (ArgumentException ex)
        {
            return new AlgorithmExecutionResult(
                "Algorithm Not Implemented",
                $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\nAlgorithm: {algorithmId}\n\n{ex.Message}",
                isError: true);
        }
    }

    // Executes the algorithm with custom parameters
    public AlgorithmExecutionResult Execute(Schedule schedule, AlgorithmId algorithmId, IAlgorithmParameters? parameters)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        try
        {
            // Start timing the algorithm execution
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            // Create the appropriate algorithm instance using the factory
            ISchedulingAlgorithm algorithm = _factory.Create(algorithmId);
            
            // Configure parameters if provided
            if (parameters is not null)
            {
                algorithm.ConfigureParameters(parameters);
            }
            
            // Execute the algorithm on the schedule
            AlgorithmExecutionResult result = algorithm.Execute(schedule);
            // Stop timing
            stopwatch.Stop();
            
            // Record the execution time for reporting
            result.ExecutionMilliseconds = (int)stopwatch.ElapsedMilliseconds;
            return result;
        }
        catch (ArgumentException ex)
        {
            // Return an error result if the algorithm cannot be created or configured
            return new AlgorithmExecutionResult(
                "Algorithm Not Implemented",
                $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\nAlgorithm: {algorithmId}\n\n{ex.Message}",
                isError: true);
        }
    }

    // Executes the algorithm with custom parameters and genetic operators
    public AlgorithmExecutionResult Execute(
        Schedule schedule, 
        AlgorithmId algorithmId, 
        IAlgorithmParameters? parameters,
        ICrossoverOperator? crossoverOperator,
        IMutationOperator? mutationOperator)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            ISchedulingAlgorithm algorithm = _factory.Create(algorithmId);
            
            // Configure parameters if provided
            if (parameters is not null)
            {
                algorithm.ConfigureParameters(parameters);
            }

            // Configure genetic operators if provided (for GeneticAlgorithm and MemeticHybrid)
            if (algorithm is GeneticAlgorithm geneticAlgorithm)
            {
                if (crossoverOperator is not null)
                {
                    geneticAlgorithm.SetCrossoverOperator(crossoverOperator);
                }

                if (mutationOperator is not null)
                {
                    geneticAlgorithm.SetMutationOperator(mutationOperator);
                }
            }
            
            AlgorithmExecutionResult result = algorithm.Execute(schedule);
            stopwatch.Stop();
            
            result.ExecutionMilliseconds = (int)stopwatch.ElapsedMilliseconds;
            return result;
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