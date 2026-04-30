namespace Job_Shop_Scheduler_Portfolio.Tests.Services;

using Job_Shop_Scheduler_Portfolio.Core.Models;
using Job_Shop_Scheduler_Portfolio.Core.Services;
using Xunit;

public class ScheduleAnalysisServiceTests
{
    [Fact]
    public void Analyse_WithValidTasks()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 4 }
        };
        string scheduleName = "Test Schedule";
        string algorithmName = "Test Algorithm";
        int makespan = 12;

        // Act
        var result = ScheduleAnalysisService.Analyse(scheduleName, algorithmName, tasks, makespan);

        // Assert
        Assert.Equal(scheduleName, result.ScheduleName);
        Assert.Equal(algorithmName, result.AlgorithmName);
        Assert.Equal(makespan, result.TotalMakespan);
        Assert.Equal(2, result.TotalJobs); // Job 1 and 2
        Assert.Equal(3, result.TotalOperations);
    }

    [Fact]
    public void Analyse_CalculatesTotalJobs()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 3 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 2 }
        };

        // Act
        var result = ScheduleAnalysisService.Analyse("Test", "Test", tasks, 10);

        // Assert
        Assert.Equal(3, result.TotalJobs);
    }

    [Fact]
    public void Analyse_CalculatesAverageTimePerJob()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 10 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 10 }
        };

        // Act
        var result = ScheduleAnalysisService.Analyse("Test", "Test", tasks, 25);

        // Assert
        // Total time: 25, Total jobs: 2, Average: 12.5
        Assert.Equal(12.5, result.AverageTimePerJob);
    }

    [Fact]
    public void Analyse_BuildsSubdivisionStatistics()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 3 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M2", ProcessingTime = 4 }
        };

        // Act
        var result = ScheduleAnalysisService.Analyse("Test", "Test", tasks, 12);

        // Assert
        Assert.NotNull(result.SubdivisionStats);
        Assert.Contains("M1", result.SubdivisionStats.Keys);
        Assert.Contains("M2", result.SubdivisionStats.Keys);
        Assert.Equal(2, result.SubdivisionStats["M1"].OperationCount);
        Assert.Equal(1, result.SubdivisionStats["M2"].OperationCount);
        Assert.Equal(8, result.SubdivisionStats["M1"].TotalProcessingTime);
        Assert.Equal(4, result.SubdivisionStats["M2"].TotalProcessingTime);
    }

    [Fact]
    public void Analyse_WithNullScheduleName()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };

        // Act
        var result = ScheduleAnalysisService.Analyse(null, "Test", tasks, 5);

        // Assert
        Assert.Null(result.ScheduleName);
        Assert.NotNull(result.AlgorithmName);
    }

    [Fact]
    public void Analyse_WithNullAlgorithmName()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };

        // Act
        var result = ScheduleAnalysisService.Analyse("Test", null, tasks, 5);

        // Assert
        Assert.Null(result.AlgorithmName);
        Assert.NotNull(result.ScheduleName);
    }

    [Fact]
    public void Analyse_WithNullTasks_ThrowsException()
    {
        // Arrange
        JSPTask[]? tasks = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ScheduleAnalysisService.Analyse("Test", "Test", tasks!, 10)
        );
    }

    [Fact]
    public void Analyse_WithEmptyTasks()
    {
        // Arrange
        var tasks = Array.Empty<JSPTask>();

        // Act
        var result = ScheduleAnalysisService.Analyse("Test", "Test", tasks, 0);

        // Assert
        Assert.Equal(0, result.TotalJobs);
        Assert.Equal(0, result.TotalOperations);
        Assert.Equal(0.0, result.AverageTimePerJob);
    }

    [Fact]
    public void Analyse_CreatesScheduledTasks()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 }
        };

        // Act
        var result = ScheduleAnalysisService.Analyse("Test", "Test", tasks, 8);

        // Assert
        Assert.NotNull(result.ScheduledTasks);
        Assert.Equal(2, result.ScheduledTasks.Count);
    }
}
