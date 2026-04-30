namespace Job_Shop_Scheduler_Portfolio.Tests.Models;

using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class JSPTaskTests
{
    [Fact]
    public void Constructor_InitialisesAllProperties()
    {
        // Arrange & Act
        var task = new JSPTask
        {
            JobId = 1,
            Operation = 1,
            SubDivision = "Machine1",
            ProcessingTime = 10
        };

        // Assert
        Assert.Equal(1, task.JobId);
        Assert.Equal(1, task.Operation);
        Assert.Equal("Machine1", task.SubDivision);
        Assert.Equal(10, task.ProcessingTime);
    }

    [Fact]
    public void Task_WithNullSubDivision()
    {
        // Arrange & Act
        var task = new JSPTask
        {
            JobId = 1,
            Operation = 1,
            SubDivision = null,
            ProcessingTime = 5
        };

        // Assert
        Assert.Null(task.SubDivision);
        Assert.Equal(1, task.JobId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Task_WithVariousProcessingTimes(int processingTime)
    {
        // Arrange & Act
        var task = new JSPTask
        {
            JobId = 1,
            Operation = 1,
            SubDivision = "Machine1",
            ProcessingTime = processingTime
        };

        // Assert
        Assert.Equal(processingTime, task.ProcessingTime);
    }
}
