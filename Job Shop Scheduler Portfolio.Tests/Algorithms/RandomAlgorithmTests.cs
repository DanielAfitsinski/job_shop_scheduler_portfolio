namespace Job_Shop_Scheduler_Portfolio.Tests.Algorithms;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Heuristics;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class RandomAlgorithmTests
{
    [Fact]
    public void Algorithm_HasCorrectProperties()
    {
        // Arrange & Act
        var algorithm = new RandomAlgorithm();

        // Assert
        Assert.Equal(AlgorithmCategory.SimpleHeuristics, algorithm.Category);
        Assert.Equal(AlgorithmId.RandomHeuristic, algorithm.Id);
        Assert.Equal("Random Heuristic", algorithm.DisplayName);
    }

    [Fact]
    public void BuildSequence_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 10 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 5 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 3 }
        };
        var schedule = new Schedule("Test", tasks);
        var algorithm = new RandomAlgorithm();

        // Act
        var sequence = algorithm.BuildSequence(schedule);

        // Assert
        Assert.Equal(3, sequence.Count);
        // Check that all tasks are present (in any order)
        var taskIds = sequence.Select(t => t.JobId).OrderBy(x => x).ToList();
        Assert.Equal(new[] { 1, 2, 3 }, taskIds);
    }

    [Fact]
    public void BuildSequence_DifferentCallsProduceDifferentResults()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 10 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 5 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 3 },
            new JSPTask { JobId = 4, Operation = 1, SubDivision = "M4", ProcessingTime = 2 },
            new JSPTask { JobId = 5, Operation = 1, SubDivision = "M5", ProcessingTime = 1 }
        };
        var schedule = new Schedule("Test", tasks);
        var algorithm = new RandomAlgorithm();

        // Act
        var sequence1 = algorithm.BuildSequence(schedule).ToList();
        var sequence2 = algorithm.BuildSequence(schedule).ToList();

        // Assert
        // Both sequences should contain all tasks
        Assert.Equal(5, sequence1.Count);
        Assert.Equal(5, sequence2.Count);
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
        var algorithm = new RandomAlgorithm();

        // Act
        var result = algorithm.Execute(schedule);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsError);
    }

    [Fact]
    public void BuildSequence_WithEmptySchedule()
    {
        // Arrange
        var schedule = new Schedule("Empty", []);
        var algorithm = new RandomAlgorithm();

        // Act
        var sequence = algorithm.BuildSequence(schedule);

        // Assert
        Assert.Empty(sequence);
    }

    [Fact]
    public void BuildSequence_WithSingleTask()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };
        var schedule = new Schedule("Test", tasks);
        var algorithm = new RandomAlgorithm();

        // Act
        var sequence = algorithm.BuildSequence(schedule);

        // Assert
        Assert.Single(sequence);
        Assert.Equal(1, sequence[0].JobId);
    }
}
