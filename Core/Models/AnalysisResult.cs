namespace Job_Shop_Scheduler_Portfolio.Core.Models;

// Contains computed statistics about a schedule's performance and composition
public class AnalysisResult
{
    // The name of the schedule that was analysed
    public string? ScheduleName { get; set; }
    // The algorithm used to compute the schedule
    public string? AlgorithmName { get; set; }
    // Total time to complete all jobs (makespan)
    public int TotalMakespan { get; set; }
    // Total number of unique jobs in the schedule
    public int TotalJobs { get; set; }
    // Total number of operations across all jobs
    public int TotalOperations { get; set; }
    // Average processing time per job
    public double AverageTimePerJob { get; set; }
    // Breakdown of operations and times by subdivision/machine
    public Dictionary<string, SubdivisionStatistics> SubdivisionStats { get; set; } = [];
    // Detailed schedule with timing information for each task
    public List<ScheduledTaskDetail> ScheduledTasks { get; set; } = [];
}

// Statistics for a single subdivision or machine
public class SubdivisionStatistics
{
    // Total number of operations performed on this subdivision
    public int OperationCount { get; set; }
    // Total processing time for all operations on this subdivision
    public int TotalProcessingTime { get; set; }
    // List of operations with their processing times
    public List<OperationDetail> Operations { get; set; } = [];
}

// Details of a single operation execution on a subdivision
public class OperationDetail
{
    // The job this operation belongs to
    public int JobId { get; set; }
    // The operation number within the job
    public int OperationNumber { get; set; }
    // Processing time for this operation
    public int ProcessingTime { get; set; }
}
