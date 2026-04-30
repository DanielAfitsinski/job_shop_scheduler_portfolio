namespace Job_Shop_Scheduler_Portfolio.Tests.Services;

using Job_Shop_Scheduler_Portfolio.Core.Models;
using Job_Shop_Scheduler_Portfolio.Core.Services;
using System.IO;
using Xunit;

public class CsvExportServiceTests
{
    [Fact]
    public void ExportToCsv_CreatesFile()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };
        var analysis = ScheduleAnalysisService.Analyse("Test Schedule", "Test Algo", tasks, 5);
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            CsvExportService.ExportToCsv(analysis, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.Contains("SCHEDULE ANALYSIS REPORT", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_ContainsSummaryInformation()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };
        var analysis = ScheduleAnalysisService.Analyse("MySchedule", "MyAlgorithm", tasks, 5);
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            CsvExportService.ExportToCsv(analysis, tempFile);

            // Assert
            var content = File.ReadAllText(tempFile);
            Assert.Contains("Schedule Name,MySchedule", content);
            Assert.Contains("Algorithm,MyAlgorithm", content);
            Assert.Contains("Total Makespan,5", content);
            Assert.Contains("Total Jobs,1", content);
            Assert.Contains("Total Operations,1", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithNullAnalysis_ThrowsException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                CsvExportService.ExportToCsv(null!, tempFile)
            );
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithNullFilePath_ThrowsException()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };
        var analysis = ScheduleAnalysisService.Analyse("Test", "Test", tasks, 5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            CsvExportService.ExportToCsv(analysis, null!)
        );
    }

    [Fact]
    public void ExportToCsv_WithMultipleTasks()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 },
            new JSPTask { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 4 }
        };
        var analysis = ScheduleAnalysisService.Analyse("Test", "Test", tasks, 12);
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            CsvExportService.ExportToCsv(analysis, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.NotEmpty(content);
            Assert.Contains("SUMMARY", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithSpecialCharactersInName()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 }
        };
        var analysis = ScheduleAnalysisService.Analyse("Test \"Schedule\", Name", "Test, Algorithm", tasks, 5);
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            CsvExportService.ExportToCsv(analysis, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.NotEmpty(content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
