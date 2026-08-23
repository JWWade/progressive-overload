using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Models.Requests;
using ProgressiveOverload.Core.Persistence;

namespace ProgressiveOverload.Core.Services;

public class WorkoutService : IWorkoutService
{
    private readonly IProgressiveOverloadRepository _repository;

    public WorkoutService(IProgressiveOverloadRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResult> LogWorkoutAsync(WorkoutSessionDraft request, CancellationToken cancellationToken = default)
    {
        if (request.SessionDate > DateOnly.FromDateTime(DateTime.Today))
        {
            return OperationResult.Failure("Session date cannot be in the future.");
        }

        if (request.Exercises.Count == 0)
        {
            return OperationResult.Failure("Workout must include at least one exercise.");
        }

        foreach (var exercise in request.Exercises)
        {
            if (string.IsNullOrWhiteSpace(exercise.ExerciseName))
            {
                return OperationResult.Failure("Every exercise must include a name.");
            }

            if (exercise.Sets.Count == 0)
            {
                return OperationResult.Failure($"Exercise {exercise.ExerciseName} must include at least one set.");
            }

            foreach (var set in exercise.Sets)
            {
                if (set.Reps <= 0)
                {
                    return OperationResult.Failure($"Reps must be greater than zero for {exercise.ExerciseName}.");
                }

                if (set.Weight < 0)
                {
                    return OperationResult.Failure($"Weight cannot be negative for {exercise.ExerciseName}.");
                }
            }
        }

        var data = await _repository.LoadAsync(cancellationToken);

        data.WorkoutSessions.Add(new WorkoutSession
        {
            Id = Guid.NewGuid(),
            SessionDate = request.SessionDate,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Exercises = request.Exercises
                .Select(e => new ExerciseEntry
                {
                    ExerciseName = e.ExerciseName.Trim(),
                    VariationName = string.IsNullOrWhiteSpace(e.VariationName) ? null : e.VariationName.Trim(),
                    Notes = string.IsNullOrWhiteSpace(e.Notes) ? null : e.Notes.Trim(),
                    Sets = e.Sets
                        .Select(s => new SetEntry
                        {
                            Reps = s.Reps,
                            Weight = s.Weight
                        })
                        .ToList()
                })
                .ToList()
        });

        await _repository.SaveAsync(data, cancellationToken);
        return OperationResult.Success();
    }
}
