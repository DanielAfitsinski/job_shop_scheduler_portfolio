namespace Job_Shop_Scheduler_Portfolio.Core.Models;

// Represents one loaded schedule and its associated tasks
public class Schedule(string name, JSPTask[] tasks)
{
    // Display name loaded from the scenario file
    public string? ScheduleName { get; set; } = name;
    // The task list used by the algorithms
    public JSPTask[] tasks = tasks;
}