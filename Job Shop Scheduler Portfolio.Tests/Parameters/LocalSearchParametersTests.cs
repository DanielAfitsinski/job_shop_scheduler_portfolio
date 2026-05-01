namespace Job_Shop_Scheduler_Portfolio.Tests.Parameters;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Xunit;

public class LocalSearchParametersTests
{
	[Fact]
	public void Constructor_InitialisesDefaultValues()
	{
		// Arrange & Act
		var parameters = new LocalSearchParameters
		{
			ConfigurationName = "Default"
		};

		// Assert
		Assert.Equal("Default", parameters.ConfigurationName);
		Assert.Equal(1000, parameters.MaxIterations);
		Assert.Equal(5, parameters.MultiStartSeeds);
	}

	[Fact]
	public void Validate_WithDefaultValues_ReturnsNull()
	{
		// Arrange
		var parameters = new LocalSearchParameters
		{
			ConfigurationName = "Default"
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Validate_WithInvalidMaxIterations_ReturnsMessage()
	{
		// Arrange
		var parameters = new LocalSearchParameters
		{
			ConfigurationName = "Default",
			MaxIterations = 0
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Equal("MaxIterations must be greater than 0", result);
	}

	[Fact]
	public void Validate_WithInvalidMultiStartSeeds_ReturnsMessage()
	{
		// Arrange
		var parameters = new LocalSearchParameters
		{
			ConfigurationName = "Default",
			MultiStartSeeds = 0
		};

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Equal("MultiStartSeeds must be greater than 0", result);
	}
}
