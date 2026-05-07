namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Generates swap neighbourhoods for local-search algorithms
public static class LocalSearchNeighbourhood
{
    // Represents a swap between two positions in the sequence
    public readonly record struct AdjacentSwapMove(int FromIndex, int ToIndex);

    // Yields every adjacent swap candidate for the current sequence
    public static IEnumerable<(AdjacentSwapMove Move, List<JSPTask> Candidate)> GenerateAdjacentSwapCandidates(
        IReadOnlyList<JSPTask> sequence)
    {
        // Iterate through each position in the sequence
        for (int index = 0; index < sequence.Count - 1; index++)
        {
            // Create a copy of the sequence so modifications don't affect future candidates
            List<JSPTask> candidate = [.. sequence];
            // Swap the task at position index with the task at position index+1
            (candidate[index], candidate[index + 1]) = (candidate[index + 1], candidate[index]);

            // Yield this candidate along with the move metadata describing the swap
            yield return (new AdjacentSwapMove(index, index + 1), candidate);
        }
    }

}