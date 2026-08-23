using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Models.Requests;
using ProgressiveOverload.Core.Persistence;

namespace ProgressiveOverload.Core.Services;

public class ProfileService : IProfileService
{
    private readonly IProgressiveOverloadRepository _repository;

    public ProfileService(IProgressiveOverloadRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResult> UpsertProfileAsync(ProfileUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return OperationResult.Failure("Name is required.");
        }

        foreach (var baseline in request.ExerciseBaselines)
        {
            if (string.IsNullOrWhiteSpace(baseline.ExerciseName))
            {
                return OperationResult.Failure("Every baseline must include an exercise name.");
            }

            if (baseline.BaselineWeight < 0)
            {
                return OperationResult.Failure($"Baseline weight cannot be negative for {baseline.ExerciseName}.");
            }

            if (baseline.BaselineReps <= 0)
            {
                return OperationResult.Failure($"Baseline reps must be greater than zero for {baseline.ExerciseName}.");
            }
        }

        var data = await _repository.LoadAsync(cancellationToken);
        var now = DateTime.UtcNow;

        data.UserProfile ??= new UserProfile
        {
            CreatedAtUtc = now
        };

        data.UserProfile.Name = request.Name.Trim();
        data.UserProfile.UpdatedAtUtc = now;
        data.UserProfile.ExerciseBaselines = request.ExerciseBaselines
            .Select(b => new ExerciseBaseline
            {
                ExerciseName = b.ExerciseName.Trim(),
                BaselineWeight = b.BaselineWeight,
                BaselineReps = b.BaselineReps,
                Notes = string.IsNullOrWhiteSpace(b.Notes) ? null : b.Notes.Trim()
            })
            .ToList();

        await _repository.SaveAsync(data, cancellationToken);
        return OperationResult.Success();
    }
}
