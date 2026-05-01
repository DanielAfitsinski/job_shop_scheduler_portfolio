namespace Job_Shop_Scheduler_Portfolio.Tests.Parameters;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Xunit;

public class TabuSearchParametersTests
{
	[Fact]
	public void Constructor_InitialisesDefaultValues()
	{
		// Arrange & Act
		var parameters = new TabuSearchParameters
		{
			ConfigurationName = "Default"
		};

		// Assert
		Assert.Equal("Default", parameters.ConfigurationName);
		Assert.Equal(500, parameters.MaxIterations);
		Assert.Equal(5, parameters.MultiStartSeeds);
		Assert.Equal(7, parameters.TabuTenure);
		Assert.Equal(50, parameters.MaxIterationsWithoutImprovement);
	}

	[Fact]
	public void Validate_WithDefaultValues_ReturnsNull()
	{
		// Arrange
		var parameters = new TabuSearchParameters
		{
			ConfigurationName = "Default"
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Validate_WithInvalidTabuTenure_ReturnsMessage()
	{
		// Arrange
		var parameters = new TabuSearchParameters
		{
			ConfigurationName = "Default",
			TabuTenure = 0
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Equal("TabuTenure must be greater than 0", result);
	}

	[Fact]
	public void Validate_WithInvalidMaxIterationsWithoutImprovement_ReturnsMessage()
	{
		// Arrange
		var parameters = new TabuSearchParameters
		{
			ConfigurationName = "Default",
			MaxIterationsWithoutImprovement = -1
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Equal("MaxIterationsWithoutImprovement must be 0 or greater", result);
	}
}
