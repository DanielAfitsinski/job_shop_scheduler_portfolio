using System;
public static class FileManager
{
    
    public static List<Schedule> LoadJobFiles()
    {
        string scenariosPath = Path.Combine(AppContext.BaseDirectory, "Scenarios");
        
        string[] csvFiles = Directory.GetFiles(scenariosPath, "*.csv");

        List<Schedule> schedules = new List<Schedule>();
        
        foreach (string file in csvFiles)
        {
            Console.WriteLine($"Loading file: {Path.GetFileName(file)}");
            
            string[] lines = File.ReadAllLines(file);

            List<JSPTask> tasks = new List<JSPTask>();

            foreach (string line in lines.Skip(1))
            {
                string[] parts = line.Split(',');
                if (parts.Length >= 4)
                {
                    int jobId = int.Parse(parts[0]);
                    int operation = int.Parse(parts[1]);
                    string subDivision = parts[2];
                    int processingTime = int.Parse(parts[3]);
                    
                    JSPTask task = new JSPTask
                    {
                        JobId = jobId,
                        Operation = operation,
                        SubDivision = subDivision,
                        ProcessingTime = processingTime
                    };
                    
                    tasks.Add(task);

                }
            }
            Console.WriteLine($"File: {Path.GetFileName(file)} successfully loaded with {tasks.Count} tasks.");
            Schedule schedule = new(Path.GetFileName(file), tasks.ToArray());
            schedules.Add(schedule);
        }

        return schedules;
    }


}