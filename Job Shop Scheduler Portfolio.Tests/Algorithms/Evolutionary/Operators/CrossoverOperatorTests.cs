namespace Job_Shop_Scheduler_Portfolio.Tests.Algorithms.Evolutionary.Operators;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Implementations;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class CrossoverOperatorTests
{
	[Fact]
	public void Name_ReturnsOrderedCrossover()
	{
		// Arrange
		var crossover = new OrderedCrossoverOperator();

		// Act
		string name = crossover.Name;

		// Assert
		Assert.Equal("Ordered Crossover", name);
	}

	[Fact]
	public void Crossover_WithIdenticalParents_ReturnsEquivalentSequence()
	{
		// Arrange
		var crossover = new OrderedCrossoverOperator();
		var parent = new List<JSPTask>
		{
			new() { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
			new() { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 },
			new() { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 4 },
			new() { JobId = 2, Operation = 2, SubDivision = "M3", ProcessingTime = 6 }
		};

		// Act
		List<JSPTask> child = crossover.Crossover(parent, parent);

		// Assert
		Assert.Equal(parent.Count, child.Count);
		for (int index = 0; index < parent.Count; index++)
		{
			Assert.Equal(parent[index].JobId, child[index].JobId);
			Assert.Equal(parent[index].Operation, child[index].Operation);
		}
	}

	[Fact]
	public void Crossover_PreservesAllTaskKeysFromParents()
	{
		// Arrange
		var crossover = new OrderedCrossoverOperator();
		var parentA = new List<JSPTask>
		{
			new() { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
			new() { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 },
			new() { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 4 },
			new() { JobId = 2, Operation = 2, SubDivision = "M3", ProcessingTime = 6 }
		};
		var parentB = new List<JSPTask>
		{
			parentA[2],
			parentA[0],
			parentA[3],
			parentA[1]
		};

		// Act
		List<JSPTask> child = crossover.Crossover(parentA, parentB);

		// Assert
		Assert.Equal(parentA.Count, child.Count);
		var expectedKeys = parentA.Select(task => $"{task.JobId}:{task.Operation}").OrderBy(key => key).ToArray();
		var actualKeys = child.Select(task => $"{task.JobId}:{task.Operation}").OrderBy(key => key).ToArray();
		Assert.Equal(expectedKeys, actualKeys);
	}
}
