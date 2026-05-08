namespace Job_Shop_Scheduler_Portfolio.Tests.Infrastructure;

using Job_Shop_Scheduler_Portfolio.Core.Models;
using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Implementations;
using System.IO;
using Xunit;

[Collection(InfrastructureScenariosCollection.Name)]
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

	[Fact]
	public void LoadJobFiles_SkipsEmptyFile()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "empty_file.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "empty_file.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Empty(loadedSchedule!.tasks);
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
	public void LoadJobFiles_SkipsFileWithOnlyInvalidLines()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "all_invalid_lines.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"invalid",
			"also,invalid",
			"too,many,columns,here,extra",
			"not,a,number,abc"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "all_invalid_lines.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Empty(loadedSchedule!.tasks);
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
	public void LoadJobFiles_SkipsLinesWithWrongColumnCount()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "wrong_columns.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"1,1,M1",
			"2,2,M2,3,extra",
			"3,3,M3,4"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "wrong_columns.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Single(loadedSchedule!.tasks);
			Assert.Equal(3, loadedSchedule.tasks[0].JobId);
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
	public void LoadJobFiles_SkipsLinesWithNonNumericJobId()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "non_numeric_jobid.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"abc,1,M1,5",
			"1,1,M1,5",
			"xyz,2,M2,3"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "non_numeric_jobid.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Single(loadedSchedule!.tasks);
			Assert.Equal(1, loadedSchedule.tasks[0].JobId);
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
	public void LoadJobFiles_SkipsLinesWithNonNumericOperation()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "non_numeric_operation.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"1,abc,M1,5",
			"2,2,M2,3",
			"3,xyz,M3,4"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "non_numeric_operation.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Single(loadedSchedule!.tasks);
			Assert.Equal(2, loadedSchedule.tasks[0].Operation);
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
	public void LoadJobFiles_SkipsLinesWithNonNumericProcessingTime()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "non_numeric_time.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"1,1,M1,abc",
			"2,2,M2,3",
			"3,3,M3,xyz"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "non_numeric_time.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Single(loadedSchedule!.tasks);
			Assert.Equal(3, loadedSchedule.tasks[0].ProcessingTime);
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
	public void LoadJobFiles_HandlesFileWithOnlyWhitespaceLines()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "whitespace_only.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"   ",
			"\t\t",
			"",
			"1,1,M1,5"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "whitespace_only.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Single(loadedSchedule!.tasks);
			Assert.Equal(1, loadedSchedule.tasks[0].JobId);
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
	public void LoadJobFiles_HandlesMixedValidAndInvalidLines()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "mixed_lines.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"1,1,M1,5",
			"invalid",
			"2,2,M2,3",
			"not,a,valid,line,with,too,many,fields",
			"3,abc,M3,4",
			"4,4,M4,5"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "mixed_lines.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Equal(3, loadedSchedule!.tasks.Length);
			Assert.Equal(1, loadedSchedule.tasks[0].JobId);
			Assert.Equal(2, loadedSchedule.tasks[1].JobId);
			Assert.Equal(4, loadedSchedule.tasks[2].JobId);
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
	public void LoadJobFiles_SkipsFileWithoutHeader()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "no_header.csv");

		File.WriteAllLines(filePath, [
			"1,1,M1,5",
			"2,2,M2,3"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "no_header.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Single(loadedSchedule!.tasks);
			Assert.Equal(2, loadedSchedule.tasks[0].JobId);
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
	public void LoadJobFiles_ReturnsEmptyListWhenNoValidFiles()
	{
		// Arrange
		Directory.CreateDirectory(ScenariosDirectory);
		string filePath = Path.Combine(ScenariosDirectory, "completely_invalid.csv");

		File.WriteAllLines(filePath, [
			"JobId,Operation,SubDivision,ProcessingTime",
			"not,valid,data,here"
		]);

		try
		{
			// Act
			List<Schedule> schedules = FileManager.LoadJobFiles();

			// Assert
			Schedule? loadedSchedule = schedules.FirstOrDefault(schedule => schedule.ScheduleName == "completely_invalid.csv");
			Assert.NotNull(loadedSchedule);
			Assert.Empty(loadedSchedule!.tasks);
		}
		finally
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
	}
}
