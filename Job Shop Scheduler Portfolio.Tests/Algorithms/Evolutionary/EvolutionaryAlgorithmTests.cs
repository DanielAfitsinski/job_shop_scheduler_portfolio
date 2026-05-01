namespace Job_Shop_Scheduler_Portfolio.Tests.Algorithms.Evolutionary;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class EvolutionaryAlgorithmTests
{
	[Fact]
	public void Category_ReturnsEvolutionary()
	{
		// Arrange
		var algorithm = new TestEvolutionaryAlgorithm();

		// Act
		AlgorithmCategory category = algorithm.Category;

		// Assert
		Assert.Equal(AlgorithmCategory.Evolutionary, category);
	}

	[Fact]
	public void ConfigureParameters_WithNull_ThrowsException()
	{
		// Arrange
		var algorithm = new TestEvolutionaryAlgorithm();

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => algorithm.ConfigureParameters(null!));
	}

	[Fact]
	public void ConfigureParameters_WithWrongType_ThrowsException()
	{
		// Arrange
		var algorithm = new TestEvolutionaryAlgorithm();
		IAlgorithmParameters wrongParameters = new HeuristicParameters();

		// Act & Assert
		Assert.Throws<ArgumentException>(() => algorithm.ConfigureParameters(wrongParameters));
	}

	[Fact]
	public void ConfigureParameters_WithInvalidEvolutionaryParameters_ThrowsException()
	{
		// Arrange
		var algorithm = new TestEvolutionaryAlgorithm();
		var invalidParameters = new EvolutionaryParameters
		{
			ConfigurationName = "Invalid",
			PopulationSize = 5
		};

		// Act & Assert
		Assert.Throws<ArgumentException>(() => algorithm.ConfigureParameters(invalidParameters));
	}

	[Fact]
	public void ConfigureParameters_WithValidParameters_UpdatesCurrentParameters()
	{
		// Arrange
		var algorithm = new TestEvolutionaryAlgorithm();
		var validParameters = new EvolutionaryParameters
		{
			ConfigurationName = "Tuned",
			PopulationSize = 20,
			Generations = 30,
			MutationRate = 0.15,
			EliteCount = 2,
			TournamentSize = 3
		};

		// Act
		algorithm.ConfigureParameters(validParameters);

		// Assert
		Assert.Same(validParameters, algorithm.Parameters);
	}

	[Fact]
	public void Execute_WithNullSchedule_ThrowsException()
	{
		// Arrange
		var algorithm = new TestEvolutionaryAlgorithm();

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => algorithm.Execute(null!));
	}

	[Fact]
	public void Execute_WithEmptySchedule_ReturnsErrorResult()
	{
		// Arrange
		var algorithm = new TestEvolutionaryAlgorithm();
		var schedule = new Schedule("Empty", []);

		// Act
		AlgorithmExecutionResult result = algorithm.Execute(schedule);

		// Assert
		Assert.True(result.IsError);
		Assert.Equal("No Tasks", result.Title);
		Assert.Equal("The selected schedule has no tasks.", result.Message);
	}

	private sealed class TestEvolutionaryAlgorithm : EvolutionaryAlgorithm
	{
		public override AlgorithmId Id => AlgorithmId.GeneticAlgorithm;

		public override string DisplayName => "Test Evolutionary";

		protected override (int populationSize, int generations) GetEffectiveSizes(int taskCount)
		{
			return (10, 10);
		}

		protected override void EvolvePopulation(EvolutionState state)
		{
		}

		protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, EvolutionState state)
		{
			return new AlgorithmExecutionResult("Test", "Test");
		}
	}
}
