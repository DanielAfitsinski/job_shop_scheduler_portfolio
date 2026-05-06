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
        int currentMakespan = ScheduleEvaluation.EvaluateMakespan(sequence, predecessorMap);
        List<JSPTask> current = sequence;

        while (iterations < parameters.MaxIterations)
        {
            iterations++;
            bool foundImprovement = false;

            foreach ((_, List<JSPTask> candidate) in LocalSearchNeighbourhood.GenerateAdjacentSwapCandidates(current))
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