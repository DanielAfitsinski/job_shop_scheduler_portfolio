namespace Job_Shop_Scheduler_Portfolio.Tests.Parameters;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Xunit;

public class EvolutionaryParametersTests
{
	[Fact]
	public void Constructor_InitialisesDefaultValues()
	{
		// Arrange & Act
		var parameters = new EvolutionaryParameters
		{
			ConfigurationName = "Default"
		};

		// Assert
		Assert.Equal("Default", parameters.ConfigurationName);
		Assert.Equal(80, parameters.PopulationSize);
		Assert.Equal(500, parameters.Generations);
		Assert.Equal(0.05, parameters.MutationRate);
		Assert.Equal(2, parameters.EliteCount);
		Assert.Equal(3, parameters.TournamentSize);
	}

	[Fact]
	public void Validate_WithDefaultValues_ReturnsNull()
	{
		// Arrange
		var parameters = new EvolutionaryParameters
		{
			ConfigurationName = "Default"
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Validate_WithInvalidPopulationSize_ReturnsMessage()
	{
		// Arrange
		var parameters = new EvolutionaryParameters
		{
			ConfigurationName = "Default",
			PopulationSize = 9
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Equal("PopulationSize must be at least 10", result);
	}

	[Fact]
	public void Validate_WithInvalidMutationRate_ReturnsMessage()
	{
		// Arrange
		var parameters = new EvolutionaryParameters
		{
			ConfigurationName = "Default",
			MutationRate = 1.5
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Equal("MutationRate must be between 0.0 and 1.0", result);
	}
}
