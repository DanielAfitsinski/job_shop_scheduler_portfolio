namespace Job_Shop_Scheduler_Portfolio.Infrastructure.Scenarios.Abstractions;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Abstraction for loading schedules into the application
public interface IScenarioProvider
{
    // Returns all schedules currently available to the UI
    IReadOnlyList<Schedule> GetSchedules();
}
