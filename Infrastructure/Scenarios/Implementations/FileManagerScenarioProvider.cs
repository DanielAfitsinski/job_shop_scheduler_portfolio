namespace Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Implementations;

using Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Scenario provider that reads from the file-manager cache
public class FileManagerScenarioProvider : IScenarioProvider
{
    // Exposes the preloaded schedule collection
    public IReadOnlyList<Schedule> GetSchedules()
    {
        return FileManager.CachedSchedules;
    }
}
