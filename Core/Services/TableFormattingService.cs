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
        sb.AppendLine();
        AppendSubdivisionSummary(sb, analysis);
        sb.AppendLine();
        
        return sb.ToString();
    }

    // Builds the summary section showing overall metrics
    private static void AppendSummarySection(StringBuilder sb, AnalysisResult analysis)
    {
        sb.AppendLine("SCHEDULE ANALYSIS RESULTS");
        sb.AppendLine("────────────────────────────────────────────────");
        sb.AppendLine($"Schedule Name:        {analysis.ScheduleName ?? "Unnamed"}");
        sb.AppendLine($"Algorithm:            {analysis.AlgorithmName ?? "Unknown"}");
        sb.AppendLine();
        sb.AppendLine($"Total Makespan:       {analysis.TotalMakespan} hours");
        sb.AppendLine($"Total Jobs:           {analysis.TotalJobs}");
        sb.AppendLine($"Total Operations:     {analysis.TotalOperations}");
        sb.AppendLine($"Average Time/Job:     {analysis.AverageTimePerJob:F2} hours");
        sb.AppendLine("────────────────────────────────────────────────");
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

        sb.AppendLine("SUBDIVISIONS / MACHINES");
        sb.AppendLine("────────────────────────────────────────────────");
        sb.AppendLine();

        foreach (var (subdivisionName, stats) in analysis.SubdivisionStats.OrderBy(s => s.Key))
        {
            sb.AppendLine($"  {subdivisionName}");
            sb.AppendLine($"    Operations:        {stats.OperationCount}");
            sb.AppendLine($"    Processing Time:   {stats.TotalProcessingTime} units");
            sb.AppendLine();
        }
    }
}
