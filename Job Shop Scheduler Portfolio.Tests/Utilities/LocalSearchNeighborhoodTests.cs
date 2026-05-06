namespace Job_Shop_Scheduler_Portfolio.Tests.Utilities;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class LocalSearchNeighbourhoodTests
{
    [Fact]
    public void GenerateAdjacentSwapCandidates_WithTwoTasks()
    {
        // Arrange
        var tasks = new List<JSPTask>
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 }
        };

        // Act
        var candidates = LocalSearchNeighbourhood.GenerateAdjacentSwapCandidates(tasks).ToList();

        // Assert
        Assert.Single(candidates);
        Assert.Equal(0, candidates[0].Move.FromIndex);
        Assert.Equal(1, candidates[0].Move.ToIndex);
        Assert.Equal(tasks[1].JobId, candidates[0].Candidate[0].JobId);
        Assert.Equal(tasks[0].JobId, candidates[0].Candidate[1].JobId);
    }

    [Fact]
    public void GenerateAdjacentSwapCandidates_WithThreeTasks()
    {
        // Arrange
        var tasks = new List<JSPTask>
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 3 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 2 }
        };

        // Act
        var candidates = LocalSearchNeighbourhood.GenerateAdjacentSwapCandidates(tasks).ToList();

        // Assert
        Assert.Equal(2, candidates.Count);
        
        // First swap: positions 0 and 1
        Assert.Equal(0, candidates[0].Move.FromIndex);
        Assert.Equal(1, candidates[0].Move.ToIndex);
        Assert.Equal(2, candidates[0].Candidate[0].JobId);
        Assert.Equal(1, candidates[0].Candidate[1].JobId);
        Assert.Equal(3, candidates[0].Candidate[2].JobId);

        // Second swap: positions 1 and 2
        Assert.Equal(1, candidates[1].Move.FromIndex);
        Assert.Equal(2, candidates[1].Move.ToIndex);
        Assert.Equal(1, candidates[1].Candidate[0].JobId);
        Assert.Equal(3, candidates[1].Candidate[1].JobId);
        Assert.Equal(2, candidates[1].Candidate[2].JobId);
    }

    [Fact]
    public void GenerateAdjacentSwapCandidates_WithSingleTask()
    {
        // Arrange
        var tasks = new List<JSPTask>
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };

        // Act
        var candidates = LocalSearchNeighbourhood.GenerateAdjacentSwapCandidates(tasks).ToList();

        // Assert
        Assert.Empty(candidates);
    }

    [Fact]
    public void GenerateAdjacentSwapCandidates_OriginSequenceNotModified()
    {
        // Arrange
        var tasks = new List<JSPTask>
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 3 }
        };
        var originalOrder = tasks.Select(t => t.JobId).ToList();

        // Act
        var _ = LocalSearchNeighbourhood.GenerateAdjacentSwapCandidates(tasks).ToList();

        // Assert - original list should be unchanged
        Assert.Equal(originalOrder, [.. tasks.Select(t => t.JobId)]);
    }

}
