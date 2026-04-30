namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Implementations;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary.Operators.Abstractions;
using Job_Shop_Scheduler_Portfolio.Core.Models;

// Simple swap mutation operator for task sequences
public class SimpleSwapMutationOperator : IMutationOperator
{
    public string Name => "Simple Swap";

    public void Mutate(List<JSPTask> chromosome)
    {
        if (chromosome.Count < 2)
        {
            // Nothing to mutate when there are fewer than two tasks
            return;
        }

        // Swap two randomly chosen positions
        int firstIndex = Random.Shared.Next(chromosome.Count);
        int secondIndex = Random.Shared.Next(chromosome.Count);
        while (secondIndex == firstIndex)
        {
            secondIndex = Random.Shared.Next(chromosome.Count);
        }

        (chromosome[firstIndex], chromosome[secondIndex]) = (chromosome[secondIndex], chromosome[firstIndex]);
    }
}
