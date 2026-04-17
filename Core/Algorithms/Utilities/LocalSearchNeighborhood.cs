namespace Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;

using Job_Shop_Scheduler_Portfolio.Core.Models;

// Generates swap neighborhoods for local-search algorithms
public static class LocalSearchNeighborhood
{
    // Represents a swap between two positions in the sequence
    public readonly record struct AdjacentSwapMove(int FromIndex, int ToIndex);

    // Yields every adjacent swap candidate for the current sequence
    public static IEnumerable<(AdjacentSwapMove Move, List<JSPTask> Candidate)> GenerateAdjacentSwapCandidates(
        IReadOnlyList<JSPTask> sequence)
    {
        // Swap each task with the task immediately after it
        for (int index = 0; index < sequence.Count - 1; index++)
        {
            // Copy the sequence so the original order stays unchanged
            List<JSPTask> candidate = [.. sequence];
            (candidate[index], candidate[index + 1]) = (candidate[index + 1], candidate[index]);

            // Return the move and its resulting candidate
            yield return (new AdjacentSwapMove(index, index + 1), candidate);
        }
    }

    // Yields every possible pairwise swap candidate for the current sequence
    public static IEnumerable<(AdjacentSwapMove Move, List<JSPTask> Candidate)> GenerateAnyPairSwapCandidates(
        IReadOnlyList<JSPTask> sequence)
    {
        // Consider every distinct pair of positions
        for (int i = 0; i < sequence.Count; i++)
        {
            for (int j = i + 1; j < sequence.Count; j++)
            {
                // Copy the sequence before swapping the pair
                List<JSPTask> candidate = [.. sequence];
                (candidate[i], candidate[j]) = (candidate[j], candidate[i]);

                // Return the move and its resulting candidate
                yield return (new AdjacentSwapMove(i, j), candidate);
            }
        }
    }
}