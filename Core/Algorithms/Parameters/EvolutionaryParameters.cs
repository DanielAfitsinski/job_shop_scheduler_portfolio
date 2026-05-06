namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Parameters for evolutionary algorithms (Genetic & Memetic)
public class EvolutionaryParameters : IEvolutionaryParameters
{
    public required string ConfigurationName { get; init; }
    public int PopulationSize { get; init; }
    public int Generations { get; init; }
    public double MutationRate { get; init; }
    public int EliteCount { get; init; }
    public int TournamentSize { get; init; }

    // Creates evolutionary parameters with default values
    public EvolutionaryParameters()
    {
        ConfigurationName = "Default";
        PopulationSize = 30;
        Generations = 80;
        MutationRate = 0.05;
        EliteCount = 2;
        TournamentSize = 3;
    }

    public string? Validate()
    {
        // Helper to validate numeric ranges
        string? CheckRange(int value, int min, int max, string name) =>
            value < min ? $"{name} must be at least {min}" :
            value > max ? $"{name} should not exceed {max} for performance reasons" :
            null;

        // Helper to check a condition
        string? CheckCondition(bool isInvalid, string message) =>
            isInvalid ? message : null;

        // Range validations
        return
            CheckRange(PopulationSize, 10, 200, "PopulationSize") ??
            CheckRange(Generations, 10, 500, "Generations") ??
            CheckCondition(MutationRate < 0.0 || MutationRate > 1.0, "MutationRate must be between 0.0 and 1.0") ??
            CheckRange(EliteCount, 1, int.MaxValue, "EliteCount") ??
            CheckRange(TournamentSize, 2, int.MaxValue, "TournamentSize") ??
            CheckCondition(EliteCount >= PopulationSize / 2, "EliteCount should be less than half the population size") ??
            CheckCondition(TournamentSize > PopulationSize / 2, "TournamentSize should not exceed half the population size");
    }
}
