namespace Job_Shop_Scheduler_Portfolio.Core.Services;

using System.Text;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Exports analysis results to CSV format for reporting and external processing
public static class CsvExportService
{
    // Exports analysis results to a CSV file at the specified path
    public static void ExportToCsv(AnalysisResult analysis, string filePath)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(filePath);

        var csv = new StringBuilder();
        BuildSummarySection(csv, analysis);
        BuildTimeScheduleSection(csv, analysis);
        BuildSubdivisionSection(csv, analysis);
        WriteToFile(csv.ToString(), filePath);
    }

    // Builds the summary section of the CSV report
    private static void BuildSummarySection(StringBuilder csv, AnalysisResult analysis)
    {
        csv.AppendLine("SCHEDULE ANALYSIS REPORT");
        csv.AppendLine();
        csv.AppendLine("SUMMARY");
        csv.AppendLine($"Schedule Name,{EscapeCsv(analysis.ScheduleName ?? "N/A")}");
        csv.AppendLine($"Algorithm,{EscapeCsv(analysis.AlgorithmName ?? "N/A")}");
        csv.AppendLine($"Total Makespan,{analysis.TotalMakespan}");
        csv.AppendLine($"Total Jobs,{analysis.TotalJobs}");
        csv.AppendLine($"Total Operations,{analysis.TotalOperations}");
        csv.AppendLine($"Average Time Per Job,{analysis.AverageTimePerJob:F2}");
        csv.AppendLine();
    }

    // Builds the time schedule section with hour-based timing and day crossings
    private static void BuildTimeScheduleSection(StringBuilder csv, AnalysisResult analysis)
    {
        if (analysis.ScheduledTasks.Count == 0)
        {
            return;
        }

        csv.AppendLine();
        csv.AppendLine("Job,Operation,Subdivision,Hours,Start Day,Start Hour,End Day,End Hour");

        foreach (var task in analysis.ScheduledTasks)
        {
            csv.AppendLine($"{task.JobId},{task.Operation},{EscapeCsv(task.SubDivision ?? "N/A")},{task.ProcessingTimeHours},{task.StartDay},{task.StartHour:D2}:00,{task.EndDay},{task.EndHour:D2}:00");
        }

        csv.AppendLine();
    }

    // Builds the subdivision breakdown section of the CSV report
    private static void BuildSubdivisionSection(StringBuilder csv, AnalysisResult analysis)
    {
        if (analysis.SubdivisionStats.Count == 0)
        {
            return;
        }

        csv.AppendLine("SUBDIVISION BREAKDOWN");
        csv.AppendLine();

        foreach (var (subdivisionName, stats) in analysis.SubdivisionStats.OrderBy(s => s.Key))
        {
            csv.AppendLine($"Subdivision: {EscapeCsv(subdivisionName)}");
            csv.AppendLine($"Operation Count,{stats.OperationCount}");
            csv.AppendLine($"Total Processing Time,{stats.TotalProcessingTime}");
            csv.AppendLine();

            if (stats.Operations.Count != 0)
            {
                csv.AppendLine("Job Id,Operation Number,Processing Time");
                foreach (var op in stats.Operations)
                {
                    csv.AppendLine($"{op.JobId},{op.OperationNumber},{op.ProcessingTime}");
                }
                csv.AppendLine();
            }
        }
    }

    // Writes the CSV content to a file, creating parent directories as needed
    private static void WriteToFile(string content, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        File.WriteAllText(filePath, content);
    }

    // Generates a timestamped filename for the export based on schedule and algorithm names
    public static string GenerateFileName(string? scheduleName, string? algorithmName)
    {
        var baseName = Path.GetFileNameWithoutExtension(scheduleName ?? "schedule");
        var algoName = algorithmName?.Replace(" ", "_") ?? "algorithm";
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        return $"{baseName}_{algoName}_{timestamp}.csv";
    }

    // Escapes special characters in CSV values to ensure valid formatting
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
