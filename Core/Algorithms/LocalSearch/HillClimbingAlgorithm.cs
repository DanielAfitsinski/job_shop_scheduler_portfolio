namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Simple local-search algorithm that greedily accepts improving swaps
public class HillClimbingAlgorithm : LocalSearchAlgorithm
{
    // Identifier used by the menu
    public override AlgorithmId Id => AlgorithmId.HillClimbing;
    // Algorithm display name
    public override string DisplayName => "Hill Climbing";

    // Searches for improvements by trying adjacent swaps until local optimum
    protected override LocalSearchResult RunSearch(List<JSPTask> sequence, Dictionary<string, string?> predecessorMap)
    {
        const int maxIterations = 200;
        int iterations = 0;
        int improvements = 0;
        int currentMakespan = ScheduleEvaluation.EvaluateMakespan(sequence, predecessorMap);
        List<JSPTask> current = sequence;

        while (iterations < maxIterations)
        {
            iterations++;
            bool foundImprovement = false;

            foreach ((_, List<JSPTask> candidate) in LocalSearchNeighborhood.GenerateAdjacentSwapCandidates(current))
            {
                int candidateMakespan = ScheduleEvaluation.EvaluateMakespan(candidate, predecessorMap);
                if (candidateMakespan < currentMakespan)
                {
                    current = candidate;
                    currentMakespan = candidateMakespan;
                    improvements++;
                    foundImprovement = true;
                    break;
                }
            }

            if (!foundImprovement)
            {
                break;
            }
        }

        return new LocalSearchResult(currentMakespan, iterations, improvements, current);
    }

    // Builds the result message summarising the search outcome
    protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, string seedName, LocalSearchResult result, List<JSPTask> bestSequence)
    {
        string message =
            $"Schedule: {schedule.ScheduleName ?? "Unnamed schedule"}\n" +
            $"Algorithm: {DisplayName}\n" +
            "Objective: Minimise makespan\n" +
            $"Task count: {schedule.tasks.Length}\n" +
            $"Best seed: {seedName}\n" +
            $"Final makespan: {result.FinalMakespan}\n" +
            $"Iterations: {result.Iterations}\n" +
            $"Improvements accepted: {result.Improvements}";

        return new AlgorithmExecutionResult(
            "Hill Climbing Result",
            message,
            computedSchedule: bestSequence,
            makespan: result.FinalMakespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }
}