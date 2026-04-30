namespace Job_Shop_Scheduler_Portfolio.Tests.Utilities;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class ScheduleEvaluationTests
{
    [Fact]
    public void BuildPredecessorMap_WithSingleJobTwoOperations()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 }
        };

        // Act
        var predecessorMap = ScheduleEvaluation.BuildPredecessorMap(tasks);

        // Assert
        Assert.Equal(2, predecessorMap.Count);
        Assert.Null(predecessorMap["1:1"]); // First operation has no predecessor
        Assert.Equal("1:1", predecessorMap["1:2"]); // Second operation's predecessor is first
    }

    [Fact]
    public void BuildPredecessorMap_WithMultipleJobs()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 4 },
            new JSPTask { JobId = 2, Operation = 2, SubDivision = "M2", ProcessingTime = 2 }
        };

        // Act
        var predecessorMap = ScheduleEvaluation.BuildPredecessorMap(tasks);

        // Assert
        Assert.Null(predecessorMap["1:1"]);
        Assert.Equal("1:1", predecessorMap["1:2"]);
        Assert.Null(predecessorMap["2:1"]);
        Assert.Equal("2:1", predecessorMap["2:2"]);
    }

    [Fact]
    public void EvaluateMakespan_WithValidSequence()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 }
        };
        var predecessorMap = ScheduleEvaluation.BuildPredecessorMap(tasks);

        // Act
        int makespan = ScheduleEvaluation.EvaluateMakespan(tasks, predecessorMap);

        // Assert
        Assert.Equal(8, makespan); // 5 + 3
    }

    [Fact]
    public void EvaluateMakespan_WithMachineConflict()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 3 }
        };
        var predecessorMap = ScheduleEvaluation.BuildPredecessorMap(tasks);

        // Act
        int makespan = ScheduleEvaluation.EvaluateMakespan(tasks, predecessorMap);

        // Assert
        Assert.Equal(8, makespan); // First task: 5, Second task (conflict on M1): 5+3=8
    }

    [Fact]
    public void EvaluateMakespan_WithDependencies()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 4 },
            new JSPTask { JobId = 2, Operation = 2, SubDivision = "M1", ProcessingTime = 2 }
        };
        // Sequence: Job1Op1, Job2Op1, Job1Op2, Job2Op2
        var sequence = new[] { tasks[0], tasks[2], tasks[1], tasks[3] };
        var predecessorMap = ScheduleEvaluation.BuildPredecessorMap(tasks);

        // Act
        int makespan = ScheduleEvaluation.EvaluateMakespan(sequence, predecessorMap);

        // Assert

        // J1O1 on M1: 0-5
        // J2O1 on M2: 0-4
        // J1O2 on M2: 5-8 (waits for J1O1)
        // J2O2 on M1: 5-7 (waits for M1 to free up at 5)
        // Makespan: 8
        Assert.Equal(8, makespan);
    }

    [Fact]
    public void EvaluateMakespan_WithEmptySequence()
    {
        // Arrange
        var tasks = Array.Empty<JSPTask>();
        var predecessorMap = ScheduleEvaluation.BuildPredecessorMap(tasks);

        // Act
        int makespan = ScheduleEvaluation.EvaluateMakespan(tasks, predecessorMap);

        // Assert
        Assert.Equal(0, makespan);
    }

    [Fact]
    public void EvaluateMakespan_WithSingleTask()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 10 }
        };
        var predecessorMap = ScheduleEvaluation.BuildPredecessorMap(tasks);

        // Act
        int makespan = ScheduleEvaluation.EvaluateMakespan(tasks, predecessorMap);

        // Assert
        Assert.Equal(10, makespan);
    }
}
