

public class Schedule
{
    public string? ScheduleName { get; set; }
    public JSPTask[] tasks = [];

    public Schedule(string name, JSPTask[] tasks)
    {
        this.ScheduleName = name;
        this.tasks = tasks;
    }


}