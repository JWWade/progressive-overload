using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Persistence;

namespace ProgressiveOverload.Core.Tests.TestDoubles;

internal sealed class InMemoryProgressiveOverloadRepository : IProgressiveOverloadRepository
{
    private ProgressiveOverloadData _data = new();

    public string DataFilePath => "in-memory";

    public Task<ProgressiveOverloadData> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Clone(_data));
    }

    public Task SaveAsync(ProgressiveOverloadData data, CancellationToken cancellationToken = default)
    {
        _data = Clone(data);
        return Task.CompletedTask;
    }

    private static ProgressiveOverloadData Clone(ProgressiveOverloadData source)
    {
        return new ProgressiveOverloadData
        {
            DataVersion = source.DataVersion,
            UserProfile = source.UserProfile is null
                ? null
                : new UserProfile
                {
                    Name = source.UserProfile.Name,
                    CreatedAtUtc = source.UserProfile.CreatedAtUtc,
                    UpdatedAtUtc = source.UserProfile.UpdatedAtUtc,
                    ExerciseBaselines = source.UserProfile.ExerciseBaselines
                        .Select(b => new ExerciseBaseline
                        {
                            ExerciseName = b.ExerciseName,
                            BaselineWeight = b.BaselineWeight,
                            BaselineReps = b.BaselineReps,
                            Notes = b.Notes
                        })
                        .ToList()
                },
            WorkoutSessions = source.WorkoutSessions
                .Select(s => new WorkoutSession
                {
                    Id = s.Id,
                    SessionDate = s.SessionDate,
                    Notes = s.Notes,
                    Exercises = s.Exercises
                        .Select(e => new ExerciseEntry
                        {
                            ExerciseName = e.ExerciseName,
                            Notes = e.Notes,
                            Sets = e.Sets
                                .Select(set => new SetEntry
                                {
                                    Reps = set.Reps,
                                    Weight = set.Weight
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
