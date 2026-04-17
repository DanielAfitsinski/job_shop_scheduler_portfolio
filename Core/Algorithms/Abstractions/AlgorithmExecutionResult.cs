namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions;

// Standard result object returned by algorithm execution
public class AlgorithmExecutionResult(
    string title,
    string message,
    bool isError = false,
    int dialogWidth = 70,
    int dialogHeight = 10)
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
}