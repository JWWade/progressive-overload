using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Models.Summaries;
using ProgressiveOverload.Core.Services;
using ProgressiveOverload.Core.Tests.TestDoubles;

namespace ProgressiveOverload.Core.Tests.Services;

public class HistorySummaryServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_AggregatesByExerciseAndTracksLastPerformedDate()
    {
        var repository = new InMemoryProgressiveOverloadRepository();

        await repository.SaveAsync(new ProgressiveOverloadData
        {
            WorkoutSessions =
            [
                new WorkoutSession
                {
                    Id = Guid.NewGuid(),
                    SessionDate = new DateOnly(2026, 8, 20),
                    Exercises =
                    [
                        new ExerciseEntry
                        {
                            ExerciseName = "Squat",
                            Sets =
                            [
                                new SetEntry { Reps = 5, Weight = 225 },
                                new SetEntry { Reps = 3, Weight = 245 }
                            ]
                        }
                    ]
                },
                new WorkoutSession
                {
                    Id = Guid.NewGuid(),
                    SessionDate = new DateOnly(2026, 8, 22),
                    Exercises =
                    [
                        new ExerciseEntry
                        {
                            ExerciseName = "squat",
                            Sets = [ new SetEntry { Reps = 4, Weight = 235 } ]
                        }
                    ]
                }
            ]
        });

        var service = new HistorySummaryService(repository);
        HistorySummary summary = await service.GetSummaryAsync();

        var item = Assert.Single(summary.Exercises);
        Assert.Equal("Squat", item.ExerciseName, ignoreCase: true);
        Assert.Equal(3, item.TotalSets);
        Assert.Equal(12, item.TotalReps);
        Assert.Equal(2800, item.TotalVolume);
        Assert.Equal(new DateOnly(2026, 8, 22), item.LastPerformedDate);
    }
}
