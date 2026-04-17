namespace Job_Shop_Scheduler_Portfolio.Core.Models;

// Individual algorithm identifiers used by the menu and runtime dispatch
public enum AlgorithmId
{
    // Shortest-processing-time heuristic
    ShortestProcessingTime,
    // Longest-processing-time heuristic
    LongestProcessingTime,
    // Random task ordering heuristic
    RandomHeuristic,
    // Basic hill-climbing local search
    HillClimbing,
    // Tabu search local search
    TabuSearch,
    // Genetic algorithm evolutionary search
    GeneticAlgorithm,
    // Memetic hybrid evolutionary search
    MemeticHybrid
}