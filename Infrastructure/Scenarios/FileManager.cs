using System;
public static class FileManager
{
    private static List<Schedule> cachedSchedules = [];

    public static IReadOnlyList<Schedule> CachedSchedules => cachedSchedules;

    public static void PreloadJobFiles()
    {
        cachedSchedules = LoadJobFiles();
    }

    public static List<Schedule> LoadJobFiles()
    {
        string scenariosPath = Path.Combine(AppContext.BaseDirectory, "Scenarios");

        if (!Directory.Exists(scenariosPath))
        {
            throw new DirectoryNotFoundException($"Scenarios directory not found: {scenariosPath}");
        }

        List<Schedule> schedules = [];

        foreach (string file in Directory.EnumerateFiles(scenariosPath, "*.csv"))
        {
            string fileName = Path.GetFileName(file);

            List<JSPTask> tasks = [];
            bool isHeader = true;

            foreach (string line in File.ReadLines(file))
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

                string[] parts = line.Split(',', 4, StringSplitOptions.TrimEntries);
                if (parts.Length != 4)
                {
                    continue;
                }

                if (!int.TryParse(parts[0], out int jobId) ||
                    !int.TryParse(parts[1], out int operation) ||
                    !int.TryParse(parts[3], out int processingTime))
                {
                    continue;
                }

                JSPTask task = new()
                {
                    JobId = jobId,
                    Operation = operation,
                    SubDivision = parts[2],
                    ProcessingTime = processingTime
                };

                tasks.Add(task);
            }

            Schedule schedule = new(fileName, [.. tasks]);
            schedules.Add(schedule);
        }

        return schedules;
    }


}