using ProgressiveOverload.Core.Models.Requests;
using ProgressiveOverload.Core.Services;
using ProgressiveOverload.Core.Tests.TestDoubles;

namespace ProgressiveOverload.Core.Tests.Services;

public class ProfileAndWorkoutServiceTests
{
    [Fact]
    public async Task UpsertProfileAsync_RejectsBlankName()
    {
        var repository = new InMemoryProgressiveOverloadRepository();
        var service = new ProfileService(repository);

        var result = await service.UpsertProfileAsync(new ProfileUpsertRequest
        {
            Name = "   "
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Name is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpsertProfileAsync_SavesTrimmedProfileAndBaselines()
    {
        var repository = new InMemoryProgressiveOverloadRepository();
        var service = new ProfileService(repository);

        var result = await service.UpsertProfileAsync(new ProfileUpsertRequest
        {
            Name = "  Josh  ",
            ExerciseBaselines =
            [
                new ExerciseBaselineInput
                {
                    ExerciseName = "  Bench Press  ",
                    BaselineWeight = 185,
                    BaselineReps = 5,
                    Notes = "  Feels solid  "
                }
            ]
        });

        var data = await repository.LoadAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(data.UserProfile);
        Assert.Equal("Josh", data.UserProfile!.Name);
        Assert.Single(data.UserProfile.ExerciseBaselines);
        Assert.Equal("Bench Press", data.UserProfile.ExerciseBaselines[0].ExerciseName);
        Assert.Equal("Feels solid", data.UserProfile.ExerciseBaselines[0].Notes);
    }

    [Fact]
    public async Task LogWorkoutAsync_RejectsFutureDate()
    {
        var repository = new InMemoryProgressiveOverloadRepository();
        var service = new WorkoutService(repository);

        var result = await service.LogWorkoutAsync(new WorkoutSessionDraft
        {
            SessionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Exercises =
            [
                new ExerciseDraft
                {
                    ExerciseName = "Squat",
                    Sets = [ new SetDraft { Reps = 5, Weight = 225 } ]
                }
            ]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Session date cannot be in the future.", result.ErrorMessage);
    }

    [Fact]
    public async Task LogWorkoutAsync_SavesWorkoutSession()
    {
        var repository = new InMemoryProgressiveOverloadRepository();
        var service = new WorkoutService(repository);

        var result = await service.LogWorkoutAsync(new WorkoutSessionDraft
        {
            SessionDate = DateOnly.FromDateTime(DateTime.Today),
            Notes = "  Lower day  ",
            Exercises =
            [
                new ExerciseDraft
                {
                    ExerciseName = "  Squat  ",
                    Notes = "  High bar  ",
                    Sets =
                    [
                        new SetDraft { Reps = 5, Weight = 225 },
                        new SetDraft { Reps = 5, Weight = 235 }
                    ]
                }
            ]
        });

        var data = await repository.LoadAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(data.WorkoutSessions);
        Assert.Equal("Lower day", data.WorkoutSessions[0].Notes);
        Assert.Single(data.WorkoutSessions[0].Exercises);
        Assert.Equal("Squat", data.WorkoutSessions[0].Exercises[0].ExerciseName);
        Assert.Equal("High bar", data.WorkoutSessions[0].Exercises[0].Notes);
        Assert.Equal(2, data.WorkoutSessions[0].Exercises[0].Sets.Count);
    }
}
