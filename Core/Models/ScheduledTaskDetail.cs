namespace Job_Shop_Scheduler_Portfolio.Core.Models;

// Represents a scheduled task with calculated start and end times including day tracking
public class ScheduledTaskDetail
{
    // The job this task belongs to
    public int JobId { get; set; }
    // The operation number within the job
    public int Operation { get; set; }
    // The machine or subdivision the task runs on
    public string? SubDivision { get; set; }
    // Processing time in hours
    public int ProcessingTimeHours { get; set; }
    // Start day of the week
    public string StartDay { get; set; } = string.Empty;
    // Start time
    public int StartHour { get; set; }
    // End day of the week
    public string EndDay { get; set; } = string.Empty;
    // End time
    public int EndHour { get; set; }
    // Total cumulative time from start of schedule in hours
    public int CumulativeStartHour { get; set; }
    // Total cumulative time at end of task in hours
    public int CumulativeEndHour { get; set; }
}
