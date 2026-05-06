namespace Job_Shop_Scheduler_Portfolio.Core.Models;

// Represents one job-shop operation to schedule
public class JSPTask
{
    // The job that owns this task
    public int JobId {get; set;}
    // The operation order within the job
    public int Operation{get;set;}
    // The machine or subdivision the task runs on
    public string? SubDivision { get; set; }
    // The processing duration for the task
    public int ProcessingTime{get;set;}
}
