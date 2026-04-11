public class AlgorithmExecutionResult(
    string title,
    string message,
    bool isError = false,
    int dialogWidth = 70,
    int dialogHeight = 10)
{
    public string Title { get; } = title;
    public string Message { get; } = message;
    public bool IsError { get; } = isError;
    public int DialogWidth { get; } = dialogWidth;
    public int DialogHeight { get; } = dialogHeight;
}