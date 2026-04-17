namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Services;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Resolves the selected algorithm and executes it for a given schedule
public class AlgorithmExecutionService
{
    // Executes the algorithm requested by the menu
    public AlgorithmExecutionResult Execute(Schedule schedule, AlgorithmId algorithmId)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        ISchedulingAlgorithm? algorithm = algorithmId switch
        {
            AlgorithmId.ShortestProcessingTime => new ShortestProcessingTimeAlgorithm(),
            AlgorithmId.LongestProcessingTime => new LongestProcessingTimeAlgorithm(),
            AlgorithmId.RandomHeuristic => new RandomAlgorithm(),
            AlgorithmId.HillClimbing => new HillClimbingAlgorithm(),
            AlgorithmId.TabuSearch => new TabuSearchAlgorithm(
                tabuTenure: 7,
                maxIterations: 500,
                useDoubleNeighborhoods: ShouldUseAnyPairNeighbourhood(schedule.tasks.Length)),
            AlgorithmId.GeneticAlgorithm => new GeneticAlgorithm(),
            AlgorithmId.MemeticHybrid => new MemeticHybridAlgorithm(),
            _ => null
        };

        if (algorithm is null)
        {
            return new AlgorithmExecutionResult(
                "Algorithm Not Implemented",
                $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\nAlgorithm: {algorithmId}\n\nThis algorithm path is not implemented yet.",
                isError: true);
        }

        return algorithm.Execute(schedule);
    }

    // Chooses any-pair tabu neighborhoods for smaller schedules
    private static bool ShouldUseAnyPairNeighbourhood(int taskCount)
    {
        return taskCount <= 40;
    }
}