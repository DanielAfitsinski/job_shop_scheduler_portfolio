namespace Job_Shop_Scheduler_Portfolio.Tests.Algorithms;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class LongestProcessingTimeAlgorithmTests
{
    [Fact]
    public void Algorithm_HasCorrectProperties()
    {
        // Arrange & Act
        var algorithm = new LongestProcessingTimeAlgorithm();

        // Assert
        Assert.Equal(AlgorithmCategory.SimpleHeuristics, algorithm.Category);
        Assert.Equal(AlgorithmId.LongestProcessingTime, algorithm.Id);
        Assert.Equal("Longest Processing Time", algorithm.DisplayName);
    }

    [Fact]
    public void BuildSequence_SortsByProcessingTimeDescending()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 10 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 5 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 3 }
        };
        var schedule = new Schedule("Test", tasks);
        var algorithm = new LongestProcessingTimeAlgorithm();

        // Act
        var sequence = algorithm.BuildSequence(schedule);

        // Assert
        Assert.Equal(3, sequence.Count);
        Assert.Equal(10, sequence[0].ProcessingTime); // Longest first
        Assert.Equal(5, sequence[1].ProcessingTime);
        Assert.Equal(3, sequence[2].ProcessingTime); // Shortest last
    }

    [Fact]
    public void BuildSequence_WithEmptySchedule()
    {
        // Arrange
        var schedule = new Schedule("Empty", []);
        var algorithm = new LongestProcessingTimeAlgorithm();

        // Act
        var sequence = algorithm.BuildSequence(schedule);

        // Assert
        Assert.Empty(sequence);
    }

    [Fact]
    public void Execute_ReturnsValidResult()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };
        var schedule = new Schedule("Test", tasks);
        var algorithm = new LongestProcessingTimeAlgorithm();

        // Act
        var result = algorithm.Execute(schedule);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsError);
    }

    [Fact]
    public void BuildSequence_WithTiedProcessingTimes()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 5 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 5 }
        };
        var schedule = new Schedule("Test", tasks);
        var algorithm = new LongestProcessingTimeAlgorithm();

        // Act
        var sequence = algorithm.BuildSequence(schedule);

        // Assert
        Assert.Equal(3, sequence.Count);
        // All have same processing time, should be sorted by JobId then Operation
        Assert.Equal(1, sequence[0].JobId);
        Assert.Equal(2, sequence[1].JobId);
        Assert.Equal(3, sequence[2].JobId);
    }
}
