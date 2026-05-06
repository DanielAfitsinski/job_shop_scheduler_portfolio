namespace Job_Shop_Scheduler_Portfolio.Core.Services;

using System.Text;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Formats analysis results as readable console tables for display
public static class TableFormattingService
{
    // Formats the analysis result as a simple, clean console report
    public static string FormatAnalysisAsTable(AnalysisResult analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        var sb = new StringBuilder();
        
        AppendSummarySection(sb, analysis);
        sb.AppendLine();
        AppendSubdivisionSection(sb, analysis);
        
        return sb.ToString();
    }

    // Formats the analysis result as a compact summary suitable for dialog display
    public static string FormatAsSummary(AnalysisResult analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        var sb = new StringBuilder();
        
        AppendSummarySection(sb, analysis);
        AppendSubdivisionSummary(sb, analysis);
        
        return sb.ToString();
    }

    // Builds the summary section showing overall metrics
    private static void AppendSummarySection(StringBuilder sb, AnalysisResult analysis)
    {
        sb.AppendLine("SCHEDULE ANALYSIS RESULTS");
        sb.AppendLine("─────────────────────────────────────────");
        sb.AppendLine($"Schedule:             {Truncate(analysis.ScheduleName ?? "Unnamed", 25)}");
        sb.AppendLine($"Algorithm:            {analysis.AlgorithmName ?? "Unknown"}");
        sb.AppendLine($"Execution Time:       {FormatExecutionTime(analysis.ExecutionMilliseconds)}");
        sb.AppendLine($"Makespan:             {analysis.TotalMakespan}h | Jobs: {analysis.TotalJobs} | Ops: {analysis.TotalOperations}");
        sb.AppendLine("─────────────────────────────────────────");
    }

    // Truncates text to maximum length
    private static string Truncate(string text, int maxLength)
    {
        return text.Length > maxLength ? text.Substring(0, maxLength - 2) + ".." : text;
    }

    // Formats execution time from milliseconds to a readable string
    private static string FormatExecutionTime(int milliseconds)
    {
        if (milliseconds < 1000)
        {
            return $"{milliseconds}ms";
        }

        double seconds = milliseconds / 1000.0;
        if (seconds < 60)
        {
            return $"{seconds:F2}s";
        }

        double minutes = seconds / 60.0;
        return $"{minutes:F2}m";
    }

    // Builds sections for each subdivision showing operation breakdown
    private static void AppendSubdivisionSection(StringBuilder sb, AnalysisResult analysis)
    {
        if (analysis.SubdivisionStats.Count == 0)
        {
            return;
        }

        sb.AppendLine();
        sb.AppendLine("SUBDIVISION / MACHINE BREAKDOWN");
        sb.AppendLine("────────────────────────────────────────────────");
        sb.AppendLine();

        foreach (var (subdivisionName, stats) in analysis.SubdivisionStats.OrderBy(s => s.Key))
        {
            AppendSubdivisionDetails(sb, subdivisionName, stats);
        }
    }

    // Builds details for a single subdivision
    private static void AppendSubdivisionDetails(StringBuilder sb, string subdivisionName, SubdivisionStatistics stats)
    {
        sb.AppendLine($"{subdivisionName}");
        sb.AppendLine($"  Operations:       {stats.OperationCount}");
        sb.AppendLine($"  Total Time:       {stats.TotalProcessingTime}");
        sb.AppendLine();
        sb.AppendLine("  Job    Op    Time");
        sb.AppendLine("  ──────────────────");

        foreach (var op in stats.Operations)
        {
            sb.AppendLine($"  {op.JobId,3}    {op.OperationNumber,2}    {op.ProcessingTime,4}");
        }

        sb.AppendLine();
    }

    // Builds a compact summary of subdivisions suitable for dialog display
    private static void AppendSubdivisionSummary(StringBuilder sb, AnalysisResult analysis)
    {
        if (analysis.SubdivisionStats.Count == 0)
        {
            return;
        }

        var subdivisions = analysis.SubdivisionStats
            .OrderByDescending(s => s.Value.OperationCount)
            .ToList();

        sb.AppendLine();
        sb.AppendLine("MACHINES");
        sb.AppendLine("─────────────────────────────────────────");

        foreach (var (subdivisionName, stats) in subdivisions)
        {
            sb.AppendLine($"{Truncate(subdivisionName, 20),-20} Ops: {stats.OperationCount,3} | Time: {stats.TotalProcessingTime,4}");
        }
    }
}
