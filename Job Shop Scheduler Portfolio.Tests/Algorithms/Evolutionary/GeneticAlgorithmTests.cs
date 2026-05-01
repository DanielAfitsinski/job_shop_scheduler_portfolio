namespace Job_Shop_Scheduler_Portfolio.Tests.Algorithms.Evolutionary;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class GeneticAlgorithmTests
{
	[Fact]
	public void Metadata_ReturnsExpectedValues()
	{
		// Arrange
		var algorithm = new GeneticAlgorithm();

		// Act & Assert
		Assert.Equal(AlgorithmId.GeneticAlgorithm, algorithm.Id);
		Assert.Equal("Genetic Algorithm", algorithm.DisplayName);
		Assert.Equal(AlgorithmCategory.Evolutionary, algorithm.Category);
	}

	[Fact]
	public void SetCrossoverOperator_WithNull_ThrowsException()
	{
		// Arrange
		var algorithm = new GeneticAlgorithm();

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => algorithm.SetCrossoverOperator(null!));
	}

	[Fact]
	public void SetMutationOperator_WithNull_ThrowsException()
	{
		// Arrange
		var algorithm = new GeneticAlgorithm();

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => algorithm.SetMutationOperator(null!));
	}

	[Fact]
	public void Parameters_DefaultConfigurationIsSet()
	{
		// Arrange
		var algorithm = new GeneticAlgorithm();

		// Act
		var parameters = Assert.IsType<EvolutionaryParameters>(algorithm.Parameters);

		// Assert
		Assert.Equal("Default", parameters.ConfigurationName);
	}

	[Fact]
	public void Execute_WithEmptySchedule_ReturnsErrorResult()
	{
		// Arrange
		var algorithm = new GeneticAlgorithm();
		var schedule = new Schedule("Empty", []);

		// Act
		AlgorithmExecutionResult result = algorithm.Execute(schedule);

		// Assert
		Assert.True(result.IsError);
		Assert.Equal("No Tasks", result.Title);
	}
}
