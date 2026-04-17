namespace Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Implementations;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Reads schedule CSV files from the application Scenarios folder
public static class FileManager
{
    // Holds the schedules that were loaded at startup
    private static List<Schedule> cachedSchedules = [];

    // Exposes the loaded schedules as a read-only list
    public static IReadOnlyList<Schedule> CachedSchedules => cachedSchedules;

    // Reloads the schedules from disk into memory
    public static void PreloadJobFiles()
    {
        cachedSchedules = LoadJobFiles();
    }

    // Reads every CSV scenario file and converts it into a schedule
    public static List<Schedule> LoadJobFiles()
    {
        string scenariosPath = GetScenariosDirectory();
        List<Schedule> schedules = [];

        foreach (string file in Directory.EnumerateFiles(scenariosPath, "*.csv"))
        {
            Schedule? schedule = LoadScheduleFromFile(file);
            if (schedule is not null)
            {
                schedules.Add(schedule);
            }
        }

        return schedules;
    }

    // Validates the scenarios directory exists, throws if missing
    private static string GetScenariosDirectory()
    {
        string scenariosPath = Path.Combine(AppContext.BaseDirectory, "Scenarios");

        if (!Directory.Exists(scenariosPath))
        {
            throw new DirectoryNotFoundException($"Scenarios directory not found: {scenariosPath}");
        }

        return scenariosPath;
    }

    // Loads a single CSV file into a Schedule object
    private static Schedule? LoadScheduleFromFile(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        List<JSPTask> tasks = [];
        bool isHeader = true;

        foreach (string line in File.ReadLines(filePath))
        {
            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            JSPTask? task = ParseCsvLine(line);
            if (task is not null)
            {
                tasks.Add(task);
            }
        }

        return new Schedule(fileName, [.. tasks]);
    }

    // Parses a single CSV line into a JSPTask, returns null if line is invalid
    private static JSPTask? ParseCsvLine(string line)
    {
        string[] parts = line.Split(',', 4, StringSplitOptions.TrimEntries);
        if (parts.Length != 4)
        {
            return null;
        }

        if (!int.TryParse(parts[0], out int jobId) ||
            !int.TryParse(parts[1], out int operation) ||
            !int.TryParse(parts[3], out int processingTime))
        {
            return null;
        }

        return new JSPTask
        {
            JobId = jobId,
            Operation = operation,
            SubDivision = parts[2],
            ProcessingTime = processingTime
        };
    }


}