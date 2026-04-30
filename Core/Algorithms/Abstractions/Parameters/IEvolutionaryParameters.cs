namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Parameters;

// Parameters for population-based evolutionary algorithms (Genetic Algorithm & Memetic Hybrid)
public interface IEvolutionaryParameters : IAlgorithmParameters
{
    // Size of the population to maintain across generations
    int PopulationSize { get; }

    // Number of generations to evolve the population
    int Generations { get; }

    // Probability of mutation applied to offspring (0.0 - 1.0)
    double MutationRate { get; }

    // Number of best individuals to preserve in each generation (elitism)
    int EliteCount { get; }

    // Number of individuals to compare in tournament selection
    int TournamentSize { get; }
}
