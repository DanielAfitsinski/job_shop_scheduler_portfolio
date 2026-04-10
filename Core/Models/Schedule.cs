public class Schedule(string name, JSPTask[] tasks)
{
    public string? ScheduleName { get; set; } = name;
    public JSPTask[] tasks = tasks;
}