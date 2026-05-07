namespace Job_Shop_Scheduler_Portfolio.Tests.Algorithms.Evolutionary;

using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Evolutionary;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Abstractions.Core;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Parameters;
using Job_Shop_Scheduler_Portfolio.Core.Algorithms.Utilities;
using Job_Shop_Scheduler_Portfolio.Core.Models;
using Xunit;

public class DeeperEvolutionaryTests
{
    private sealed class TestEvolution : EvolutionaryAlgorithm
    {
        public TestEvolution()
        {
            parameters = new EvolutionaryParameters { ConfigurationName = "Test", PopulationSize = 4, Generations = 2, MutationRate = 0.0, EliteCount = 1, TournamentSize = 2 };
        }

        public static List<List<JSPTask>> CallBuildInitialPopulation(IReadOnlyList<JSPTask> seed, IReadOnlyDictionary<string, string?> pred, int size)
            => BuildInitialPopulation(seed, pred, size);

        public static List<JSPTask> CallRepairToFeasibleOrder(IReadOnlyList<JSPTask> seq, IReadOnlyDictionary<string, string?> pred)
            => SequenceRepair.RepairToFeasibleOrder(seq, pred);

        public static List<int> CallScorePopulation(IReadOnlyList<List<JSPTask>> population, IReadOnlyDictionary<string, string?> pred, out int evaluations)
        {
            evaluations = 0;
            var scored = ScorePopulation(population, pred, ref evaluations);
            return [.. scored.Select(s => s.Makespan)];
        }

        public override AlgorithmId Id => AlgorithmId.GeneticAlgorithm;
        public override string DisplayName => "TestEvolution";

        protected override (int populationSize, int generations) GetEffectiveSizes(int taskCount) => (parameters.PopulationSize, parameters.Generations);
        protected override void EvolvePopulation(EvolutionState state) { }
        protected override AlgorithmExecutionResult BuildResultMessage(Schedule schedule, EvolutionState state) => new("Test", "Test");
    }

    [Fact]
    public void BuildInitialPopulation_IncludesSeedAndRespectsSize()
    {
        // Arrange
        var seed = new List<JSPTask>
        {
            new() { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new() { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 },
            new() { JobId = 2, Operation = 1, SubDivision = "M1", ProcessingTime = 4 }
        };

        var pred = ScheduleEvaluation.BuildPredecessorMap(seed);
        var subject = new TestEvolution();

        // Act
        var population = TestEvolution.CallBuildInitialPopulation(seed, pred, 5);

        // Assert
        Assert.Equal(5, population.Count);
        Assert.Equal(seed.Count, population[0].Count);
        // Ensure all individuals are permutations of the same keys
        var expectedKeys = seed.Select(t => $"{t.JobId}:{t.Operation}").OrderBy(k => k).ToArray();
        foreach (var individual in population)
        {
            var keys = individual.Select(t => $"{t.JobId}:{t.Operation}").OrderBy(k => k).ToArray();
            Assert.Equal(expectedKeys, keys);
        }
    }

    [Fact]
    public void RepairToFeasibleOrder_ReordersAccordingToPredecessors()
    {
        // Arrange: job 1 has operation 1 then 2
        var a = new JSPTask { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 2 };
        var b = new JSPTask { JobId = 1, Operation = 2, SubDivision = "M2", ProcessingTime = 3 };
        var sequence = new List<JSPTask> { b, a };
        var pred = ScheduleEvaluation.BuildPredecessorMap([a, b]);
        var subject = new TestEvolution();

        // Act
        var repaired = TestEvolution.CallRepairToFeasibleOrder(sequence, pred);

        // Assert: operation 1 must appear before operation 2 for the same job
        Assert.Equal(2, repaired.Count);
        int idxOp1 = repaired.FindIndex(t => t.JobId == 1 && t.Operation == 1);
        int idxOp2 = repaired.FindIndex(t => t.JobId == 1 && t.Operation == 2);
        Assert.True(idxOp1 >= 0 && idxOp2 >= 0);
        Assert.True(idxOp1 < idxOp2, "Operation 1 should appear before Operation 2 after repair");
    }

    [Fact]
    public void ScorePopulation_ComputesMakespansAndIncrementsEvaluations()
    {
        // Arrange
        var tasks = new JSPTask[]
        {
            new() { JobId = 1, Operation = 1, SubDivision = "M1", ProcessingTime = 5 },
            new() { JobId = 2, Operation = 1, SubDivision = "M2", ProcessingTime = 3 }
        };

        var pred = ScheduleEvaluation.BuildPredecessorMap(tasks);
        var pop = new List<List<JSPTask>> { new(tasks), new(tasks.Reverse()) };
        var subject = new TestEvolution();

        // Act
        int evals;
        var makespans = TestEvolution.CallScorePopulation(pop, pred, out evals);

        // Assert
        Assert.Equal(2, makespans.Count);
        Assert.True(evals >= 2);
        Assert.Equal([.. makespans.OrderBy(m => m)], [.. makespans]);
    }

}
