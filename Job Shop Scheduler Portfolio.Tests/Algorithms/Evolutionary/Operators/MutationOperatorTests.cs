namespace Job_Shop_Scheduler_Portfolio.Tests.Algorithms.Evolutionary.Operators;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Implementations;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class MutationOperatorTests
{
	[Fact]
	public void Name_ReturnsSimpleSwap()
	{
		// Arrange
		var mutation = new SimpleSwapMutationOperator();

		// Act
		string name = mutation.Name;

		// Assert
		Assert.Equal("Simple Swap", name);
	}

	[Fact]
	public void Mutate_WithSingleTask_DoesNotChangeChromosome()
	{
		// Arrange
		var mutation = new SimpleSwapMutationOperator();
		var task = new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 };
		var chromosome = new List<JSPTask> { task };

		// Act
		mutation.Mutate(chromosome);

		// Assert
		Assert.Single(chromosome);
		Assert.Same(task, chromosome[0]);
	}

	[Fact]
	public void Mutate_WithMultipleTasks_KeepsAllTasksAndChangesOrder()
	{
		// Arrange
		var mutation = new SimpleSwapMutationOperator();
		var taskA = new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 };
		var taskB = new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 };
		var taskC = new JSPTask { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 4 };
		var chromosome = new List<JSPTask> { taskA, taskB, taskC };
		JSPTask[] originalOrder = [.. chromosome];

		// Act
		mutation.Mutate(chromosome);

		// Assert
		Assert.Equal(originalOrder.Length, chromosome.Count);
		Assert.Equal(originalOrder.OrderBy(task => $"{task.JobId}:{task.Operation}"), chromosome.OrderBy(task => $"{task.JobId}:{task.Operation}"));
		Assert.NotEqual(originalOrder, chromosome);
	}
}
