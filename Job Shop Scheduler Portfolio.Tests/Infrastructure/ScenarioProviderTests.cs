namespace Job_Shop_Scheduler_Portfolio.Tests.Infrastructure;

using Job_Shop_Scheduler_Portfolio.Core.Models;
using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Implementations;
using System.IO;
using Xunit;

[Collection(InfrastructureScenariosCollection.Name)]
public class ScenarioProviderTests
{
	private static readonly string ScenariosDirectory = Path.Combine(AppContext.BaseDirectory, "Scenarios");
	private static readonly string ScenarioFileName = "scenario_provider_tests.csv";

	[Fact]
	public void GetSchedules_ReturnsCachedSchedulesFromFileManager()
	{
		// Arrange
		var provider = new FileManagerScenarioProvider();
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, ScenarioFileName);

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"1,1,M1,5"
		]);

		try
		{
			// Act
			FileManager.PreloadJobFiles();
			IReadOnlyList<Schedule> schedules = provider.GetSchedules();

			// Assert
			Assert.Same(FileManager.CachedSchedules, schedules);
			Assert.Single(schedules);
			Assert.Equal(ScenarioFileName, schedules[0].ScheduleName);
			Assert.Single(schedules[0].tasks);
			Assert.Equal(1, schedules[0].tasks[0].JobId);
			Assert.Equal(1, schedules[0].tasks[0].Operation);
			Assert.Equal("M1", schedules[0].tasks[0].SubDivision);
			Assert.Equal(5, schedules[0].tasks[0].ProcessingTime);
		}
		finally
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			FileManager.PreloadJobFiles();
		}
	}
}
