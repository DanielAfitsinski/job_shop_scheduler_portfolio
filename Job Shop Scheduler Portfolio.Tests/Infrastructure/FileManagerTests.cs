namespace Job_Shop_Scheduler_Portfolio.Tests.Infrastructure;

using Job_Shop_Scheduler_Portfolio.Core.Models;
using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Implementations;
using System.IO;
using Xunit;

public class FileManagerTests
{
	private static readonly string ScenariosDirectory = Path.Combine(AppContext.BaseDirectory, "Scenarios");
	private static readonly string LoadTestFileName = "file_manager_tests.csv";
	private static readonly string CacheTestFileName = "file_manager_cache_tests.csv";

	[Fact]
	public void LoadJobFiles_LoadsSchedulesFromCsvFiles()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, LoadTestFileName);

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"1,1,M1,5",
			string.Empty,
			"invalid,line",
			"2,2,M2,3"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == LoadTestFileName);
			Assert.NotNull(loadedSchedule);
			Assert.Equal(2, loadedSchedule!.tasks.Length);
			Assert.Equal(1, loadedSchedule.tasks[0].JobId);
			Assert.Equal(1, loadedSchedule.tasks[0].Operation);
			Assert.Equal("M1", loadedSchedule.tasks[0].SubDivision);
			Assert.Equal(5, loadedSchedule.tasks[0].ProcessingTime);
			Assert.Equal(2, loadedSchedule.tasks[1].JobId);
			Assert.Equal(2, loadedSchedule.tasks[1].Operation);
			Assert.Equal("M2", loadedSchedule.tasks[1].SubDivision);
			Assert.Equal(3, loadedSchedule.tasks[1].ProcessingTime);
		}
		finally
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
	}

	[Fact]
	public void PreloadJobFiles_UpdatesCachedSchedules()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, CacheTestFileName);

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"3,1,M3,8"
		]);

		try
		{
			// Act
			FileManager.PreloadJobFiles();

			// Assert
			Assert.Contains(FileManager.CachedSchedules, schedule => schedule.ScheduleName == CacheTestFileName);
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
