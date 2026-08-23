using ProgressiveOverload.Core.Models.Requests;
using ProgressiveOverload.Core.Services;
using ProgressiveOverload.Core.Tests.TestDoubles;

namespace ProgressiveOverload.Core.Tests.Services;

public class DefinitionServiceTests
{
    [Fact]
    public async Task UpsertExerciseDefinitionAsync_RejectsMoreThanTwoVariations()
    {
        var repository = new InMemoryProgressiveOverloadRepository();
        var service = new DefinitionService(repository);

        var result = await service.UpsertExerciseDefinitionAsync(new UpsertExerciseDefinitionRequest
        {
            CategoryName = "Push",
            ExerciseName = "Flat BB Press",
            Variations =
            [
                new ExerciseVariationInput { Name = "Strength", VolumeMultiplier = 1 },
                new ExerciseVariationInput { Name = "Hypertrophy", VolumeMultiplier = 1 },
                new ExerciseVariationInput { Name = "Endurance", VolumeMultiplier = 1 }
            ]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("A maximum of two variations is supported.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpsertExerciseDefinitionAsync_SavesCategoryExerciseAndVariations()
    {
        var repository = new InMemoryProgressiveOverloadRepository();
        var service = new DefinitionService(repository);

        var result = await service.UpsertExerciseDefinitionAsync(new UpsertExerciseDefinitionRequest
        {
            CategoryName = "  Push  ",
            ExerciseName = "  Flat BB Press  ",
            Notes = "  Main chest press  ",
            Variations =
            [
                new ExerciseVariationInput
                {
                    Name = "  Strength  ",
                    VolumeMultiplier = 1.1m,
                    Notes = "  3-5 reps  "
                },
                new ExerciseVariationInput
                {
                    Name = "  Endurance  ",
                    VolumeMultiplier = 0.9m,
                    Notes = "  12+ reps  "
                }
            ]
        });

        var catalog = await service.GetCatalogAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(catalog.Categories);
        Assert.Equal("Push", catalog.Categories[0].Name);
        Assert.Single(catalog.Exercises);

        var exercise = catalog.Exercises[0];
        Assert.Equal("Flat BB Press", exercise.Name);
        Assert.Equal("Push", exercise.CategoryName);
        Assert.Equal("Main chest press", exercise.Notes);
        Assert.Equal(2, exercise.Variations.Count);
        Assert.Equal("Strength", exercise.Variations[0].Name);
        Assert.Equal(1.1m, exercise.Variations[0].VolumeMultiplier);
        Assert.Equal("Endurance", exercise.Variations[1].Name);
        Assert.Equal(0.9m, exercise.Variations[1].VolumeMultiplier);
    }
}
