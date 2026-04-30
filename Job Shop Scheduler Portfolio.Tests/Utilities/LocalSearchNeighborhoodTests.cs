namespace Job_Shop_Scheduler_Portfolio.Tests.Utilities;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class LocalSearchNeighborhoodTests
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
        var candidates = LocalSearchNeighborhood.GenerateAdjacentSwapCandidates(tasks).ToList();

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
        var candidates = LocalSearchNeighborhood.GenerateAdjacentSwapCandidates(tasks).ToList();

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
        var candidates = LocalSearchNeighborhood.GenerateAdjacentSwapCandidates(tasks).ToList();

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
        var _ = LocalSearchNeighborhood.GenerateAdjacentSwapCandidates(tasks).ToList();

        // Assert - original list should be unchanged
        Assert.Equal(originalOrder, [.. tasks.Select(t => t.JobId)]);
    }

    [Fact]
    public void GenerateAnyPairSwapCandidates_WithThreeTasks()
    {
        // Arrange
        var tasks = new List<JSPTask>
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 3 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 2 }
        };

        // Act
        var candidates = LocalSearchNeighborhood.GenerateAnyPairSwapCandidates(tasks).ToList();

        // Assert
        // For 3 tasks: (0,1), (0,2), (1,2) = 3 combinations
        Assert.Equal(3, candidates.Count);

        // Check first swap (0, 1)
        Assert.Equal(0, candidates[0].Move.FromIndex);
        Assert.Equal(1, candidates[0].Move.ToIndex);
        Assert.Equal(new[] { 2, 1, 3 }, candidates[0].Candidate.Select(t => t.JobId).ToArray());

        // Check second swap (0, 2)
        Assert.Equal(0, candidates[1].Move.FromIndex);
        Assert.Equal(2, candidates[1].Move.ToIndex);
        Assert.Equal(new[] { 3, 2, 1 }, candidates[1].Candidate.Select(t => t.JobId).ToArray());

        // Check third swap (1, 2)
        Assert.Equal(1, candidates[2].Move.FromIndex);
        Assert.Equal(2, candidates[2].Move.ToIndex);
        Assert.Equal(new[] { 1, 3, 2 }, candidates[2].Candidate.Select(t => t.JobId).ToArray());
    }

    [Fact]
    public void GenerateAnyPairSwapCandidates_WithTwoTasks()
    {
        // Arrange
        var tasks = new List<JSPTask>
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 3 }
        };

        // Act
        var candidates = LocalSearchNeighborhood.GenerateAnyPairSwapCandidates(tasks).ToList();

        // Assert
        Assert.Single(candidates);
        Assert.Equal(0, candidates[0].Move.FromIndex);
        Assert.Equal(1, candidates[0].Move.ToIndex);
    }

    [Fact]
    public void GenerateAnyPairSwapCandidates_OriginSequenceNotModified()
    {
        // Arrange
        var tasks = new List<JSPTask>
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 3 },
            new JSPTask { JobId = 3, Operation = 1, SubDivision = "M3", ProcessingTime = 2 }
        };
        var originalOrder = tasks.Select(t => t.JobId).ToList();

        // Act
        var _ = LocalSearchNeighborhood.GenerateAnyPairSwapCandidates(tasks).ToList();

        // Assert - original list should be unchanged
        Assert.Equal(originalOrder, tasks.Select(t => t.JobId).ToList());
    }
}
