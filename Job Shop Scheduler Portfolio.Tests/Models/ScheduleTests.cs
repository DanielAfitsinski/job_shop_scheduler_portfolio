namespace Job_Shop_Scheduler_Portfolio.Tests.Models;

using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class ScheduleTests
{
    [Fact]
    public void Constructor_InitialisesNameAndTasks()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 }
        };
        string scheduleName = "Test Schedule";

        // Act
        var schedule = new Schedule(scheduleName, tasks);

        // Assert
        Assert.Equal(scheduleName, schedule.ScheduleName);
        Assert.Equal(tasks.Length, schedule.tasks.Length);
        Assert.Same(tasks, schedule.tasks);
    }

    [Fact]
    public void Constructor_WithEmptyTasks()
    {
        // Arrange
        var tasks = Array.Empty<JSPTask>();
        string scheduleName = "Empty Schedule";

        // Act
        var schedule = new Schedule(scheduleName, tasks);

        // Assert
        Assert.Equal(scheduleName, schedule.ScheduleName);
        Assert.Empty(schedule.tasks);
    }

    [Fact]
    public void Constructor_WithMultipleTasks()
    {
        // Arrange
        var tasks = new JSPTask[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = new JSPTask
            {
                JobId = i / 2,
                Operation = i % 2,
                SubDivision = $"M{i % 3}",
                ProcessingTime = (i + 1) * 2
            };
        }

        // Act
        var schedule = new Schedule("Test", tasks);

        // Assert
        Assert.Equal(10, schedule.tasks.Length);
        Assert.Equal(0, schedule.tasks[0].JobId);
        Assert.Equal(20, schedule.tasks[9].ProcessingTime);
    }
}
