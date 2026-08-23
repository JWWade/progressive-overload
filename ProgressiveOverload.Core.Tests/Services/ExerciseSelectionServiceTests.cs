using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Models.Definitions;
using ProgressiveOverload.Core.Services;
using ProgressiveOverload.Core.Tests.TestDoubles;

namespace ProgressiveOverload.Core.Tests.Services;

public class ExerciseSelectionServiceTests
{
    [Fact]
    public async Task RecommendNextVariationAsync_ReturnsNullWhenCategoryHasNoDefinitions()
    {
        var repository = new InMemoryProgressiveOverloadRepository();
        var service = new ExerciseSelectionService(repository);

        var recommendation = await service.RecommendNextVariationAsync("Push");

        Assert.Null(recommendation);
    }

    [Fact]
    public async Task RecommendNextVariationAsync_PrefersLowerScoreUsingVolumeAndRecency()
    {
        var repository = new InMemoryProgressiveOverloadRepository();

        await repository.SaveAsync(new ProgressiveOverloadData
        {
            ExerciseCatalog = new ExerciseCatalog
            {
                Categories = [ new MovementCategory { Name = "Push" } ],
                Exercises =
                [
                    new ExerciseDefinition
                    {
                        Name = "Flat BB Press",
                        CategoryName = "Push",
                        Variations =
                        [
                            new ExerciseVariationDefinition { Name = "Strength", VolumeMultiplier = 1.2m },
                            new ExerciseVariationDefinition { Name = "Endurance", VolumeMultiplier = 0.8m }
                        ]
                    }
                ]
            },
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
                            ExerciseName = "Flat BB Press",
                            VariationName = "Strength",
                            Sets = [ new SetEntry { Reps = 3, Weight = 230 } ]
                        }
                    ]
                },
                new WorkoutSession
                {
                    Id = Guid.NewGuid(),
                    SessionDate = new DateOnly(2026, 8, 23),
                    Exercises =
                    [
                        new ExerciseEntry
                        {
                            ExerciseName = "Flat BB Press",
                            VariationName = "Endurance",
                            Sets = [ new SetEntry { Reps = 12, Weight = 155 } ]
                        }
                    ]
                }
            ]
        });

        var service = new ExerciseSelectionService(repository);
        var recommendation = await service.RecommendNextVariationAsync("Push", new DateOnly(2026, 8, 24));

        Assert.NotNull(recommendation);
        Assert.Equal("Flat BB Press", recommendation!.ExerciseName);
        Assert.Equal("Strength", recommendation.VariationName);
    }
}
