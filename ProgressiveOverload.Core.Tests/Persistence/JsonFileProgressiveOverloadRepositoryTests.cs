using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Persistence;

namespace ProgressiveOverload.Core.Tests.Persistence;

public class JsonFileProgressiveOverloadRepositoryTests
{
    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsData()
    {
        var folderName = $"ProgressiveOverloadTests_{Guid.NewGuid():N}";
        var fileName = "data.json";
        var repository = new JsonFileProgressiveOverloadRepository(folderName, fileName);

        try
        {
            var expected = new ProgressiveOverloadData
            {
                DataVersion = 7,
                UserProfile = new UserProfile
                {
                    Name = "Josh",
                    CreatedAtUtc = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 8, 23, 0, 0, 0, DateTimeKind.Utc),
                    ExerciseBaselines =
                    [
                        new ExerciseBaseline
                        {
                            ExerciseName = "Deadlift",
                            BaselineWeight = 315,
                            BaselineReps = 5
                        }
                    ]
                },
                WorkoutSessions =
                [
                    new WorkoutSession
                    {
                        Id = Guid.NewGuid(),
                        SessionDate = new DateOnly(2026, 8, 23),
                        Exercises =
                        [
                            new ExerciseEntry
                            {
                                ExerciseName = "Deadlift",
                                Sets = [ new SetEntry { Reps = 5, Weight = 315 } ]
                            }
                        ]
                    }
                ]
            };

            await repository.SaveAsync(expected);
            var actual = await repository.LoadAsync();

            Assert.Equal(7, actual.DataVersion);
            Assert.NotNull(actual.UserProfile);
            Assert.Equal("Josh", actual.UserProfile!.Name);
            Assert.Single(actual.UserProfile.ExerciseBaselines);
            Assert.Equal("Deadlift", actual.UserProfile.ExerciseBaselines[0].ExerciseName);
            Assert.Single(actual.WorkoutSessions);
            Assert.Equal(new DateOnly(2026, 8, 23), actual.WorkoutSessions[0].SessionDate);
            Assert.Single(actual.WorkoutSessions[0].Exercises);
        }
        finally
        {
            var directory = Path.GetDirectoryName(repository.DataFilePath);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
