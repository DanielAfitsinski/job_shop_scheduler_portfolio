namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.LocalSearch;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
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
        int iterations = 0;
        int improvements = 0;
        // Evaluate the initial makespan
        int currentMakespan = ScheduleEvaluation.EvaluateMakespan(sequence, predecessorMap);
        List<JSPTask> current = sequence;

    // Continue searching until we exceed iteration limit or reach a local optimum
        while (iterations < parameters.MaxIterations)
        {
            iterations++;
            // Track whether this iteration found any improvement
            bool foundImprovement = false;

            // Try swapping each pair of adjacent tasks in the sequence
            // The neighbourhood consists of all solutions reachable by one adjacent swap
            foreach ((_, List<JSPTask> candidate) in LocalSearchNeighbourhood.GenerateAdjacentSwapCandidates(current))
            {
                // Evaluate the candidate solution
                int candidateMakespan = ScheduleEvaluation.EvaluateMakespan(candidate, predecessorMap);
                // Accept the first improvement found
                if (candidateMakespan < currentMakespan)
                {
                    // Move to this better solution
                    current = candidate;
                    currentMakespan = candidateMakespan;
                    improvements++;
                    foundImprovement = true;
                    // Stop searching neighbours and start the next iteration
                    break;
                }
            }

            // If no improving move was found, we've reached a local optimum
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
        string message = AlgorithmResultFormatter.BuildStandardMessage(
            schedule,
            DisplayName,
            schedule.tasks.Length,
            result.FinalMakespan);

        return new AlgorithmExecutionResult(
            "Hill Climbing Result",
            message,
            computedSchedule: bestSequence,
            makespan: result.FinalMakespan,
            scheduleName: schedule.ScheduleName,
            algorithmName: DisplayName);
    }
}