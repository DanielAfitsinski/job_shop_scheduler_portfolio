namespace Job_Shop_Scheduler_Portfolio.Tests.Algorithms;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class ShortestProcessingTimeAlgorithmTests
{
    [Fact]
    public void Algorithm_HasCorrectProperties()
    {
        // Arrange & Act
        var algorithm = new ShortestProcessingTimeAlgorithm();

        // Assert
        Assert.Equal(AlgorithmCategory.SimpleHeuristics, algorithm.Category);
        Assert.Equal(AlgorithmId.ShortestProcessingTime, algorithm.Id);
        Assert.Equal("Shortest Processing Time", algorithm.DisplayName);
    }

    [Fact]
    public void BuildSequence_SortsByProcessingTime()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 10 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 5 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 3 }
        };
        var schedule = new Schedule("Test", tasks);
        var algorithm = new ShortestProcessingTimeAlgorithm();

        // Act
        var sequence = algorithm.BuildSequence(schedule);

        // Assert
        Assert.Equal(3, sequence.Count);
        Assert.Equal(3, sequence[0].ProcessingTime); // Shortest first
        Assert.Equal(5, sequence[1].ProcessingTime);
        Assert.Equal(10, sequence[2].ProcessingTime); // Longest last
    }

    [Fact]
    public void BuildSequence_WithNullSchedule_ThrowsException()
    {
        // Arrange
        var algorithm = new ShortestProcessingTimeAlgorithm();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => algorithm.BuildSequence(null!));
    }

    [Fact]
    public void BuildSequence_WithEmptySchedule()
    {
        // Arrange
        var schedule = new Schedule("Empty", []);
        var algorithm = new ShortestProcessingTimeAlgorithm();

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
        var algorithm = new ShortestProcessingTimeAlgorithm();

        // Act
        var result = algorithm.Execute(schedule);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsError);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public void Execute_WithEmptySchedule_ReturnsError()
    {
        // Arrange
        var schedule = new Schedule("Empty", []);
        var algorithm = new ShortestProcessingTimeAlgorithm();

        // Act
        var result = algorithm.Execute(schedule);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("No Tasks", result.Title);
    }

    [Fact]
    public void ConfigureParameters_WithValidParameters()
    {
        // Arrange
        var algorithm = new ShortestProcessingTimeAlgorithm();
        var newParams = new HeuristicParameters { ConfigurationName = "Custom" };

        // Act
        algorithm.ConfigureParameters(newParams);

        // Assert
        Assert.Equal("Custom", ((HeuristicParameters)algorithm.Parameters).ConfigurationName);
    }

    [Fact]
    public void ConfigureParameters_WithNullParameters_ThrowsException()
    {
        // Arrange
        var algorithm = new ShortestProcessingTimeAlgorithm();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => algorithm.ConfigureParameters(null!));
    }

    [Fact]
    public void ConfigureParameters_WithWrongParameterType_ThrowsException()
    {
        // Arrange
        var algorithm = new ShortestProcessingTimeAlgorithm();
        var wrongParams = new LocalSearchParameters { ConfigurationName = "Wrong" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => algorithm.ConfigureParameters(wrongParams));
    }
}
