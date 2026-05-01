namespace Job_Shop_Scheduler_Portfolio.Tests.Parameters;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Xunit;

public class HeuristicParametersTests
{
	[Fact]
	public void Constructor_InitialisesDefaultConfigurationName()
	{
		// Arrange & Act
		var parameters = new HeuristicParameters();

		// Assert
		Assert.Equal("Default", parameters.ConfigurationName);
	}

	[Fact]
	public void Validate_ReturnsNull()
	{
		// Arrange
		var parameters = new HeuristicParameters();

		// Act
		string? result = parameters.Validate();

		// Assert
		Assert.Null(result);
	}
}
