namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Standard result object returned by algorithm execution
public class AlgorithmExecutionResult(
    string title,
    string message,
    bool isError = false,
    int dialogWidth = 70,
    int dialogHeight = 10,
    IReadOnlyList<JSPTask>? computedSchedule = null,
    int makespan = 0,
    string? scheduleName = null,
    string? algorithmName = null)
{
    // Dialog title shown to the user
    public string Title { get; } = title;
    // Detailed result text shown to the user
    public string Message { get; } = message;
    // Indicates whether the result represents an error state
    public bool IsError { get; } = isError;
    // Preferred dialog width for the UI
    public int DialogWidth { get; } = dialogWidth;
    // Preferred dialog height for the UI
    public int DialogHeight { get; } = dialogHeight;
    // The computed ordered schedule (if available)
    public IReadOnlyList<JSPTask>? ComputedSchedule { get; } = computedSchedule;
    // The makespan of the computed schedule (if available)
    public int Makespan { get; } = makespan;
    // The name of the schedule that was processed
    public string? ScheduleName { get; } = scheduleName;
    // The name of the algorithm that was executed
    public string? AlgorithmName { get; } = algorithmName;
}