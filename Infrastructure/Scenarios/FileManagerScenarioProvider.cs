public class FileManagerScenarioProvider : IScenarioProvider
{
    public IReadOnlyList<Schedule> GetSchedules()
    {
        return FileManager.CachedSchedules;
    }
}
