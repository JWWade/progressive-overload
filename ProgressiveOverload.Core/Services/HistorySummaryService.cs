using ProgressiveOverload.Core.Models.Summaries;
using ProgressiveOverload.Core.Persistence;

namespace ProgressiveOverload.Core.Services;

public class HistorySummaryService : IHistorySummaryService
{
    private readonly IProgressiveOverloadRepository _repository;

    public HistorySummaryService(IProgressiveOverloadRepository repository)
    {
        _repository = repository;
    }

    public async Task<HistorySummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var data = await _repository.LoadAsync(cancellationToken);
        var byExercise = new Dictionary<string, ExerciseSummaryItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var session in data.WorkoutSessions)
        {
            foreach (var exercise in session.Exercises)
            {
                var key = exercise.ExerciseName.Trim();
                if (!byExercise.TryGetValue(key, out var summary))
                {
                    summary = new ExerciseSummaryItem
                    {
                        ExerciseName = key,
                        LastPerformedDate = session.SessionDate
                    };
                    byExercise[key] = summary;
                }

                summary.TotalSets += exercise.Sets.Count;
                summary.TotalReps += exercise.Sets.Sum(s => s.Reps);
                summary.TotalVolume += exercise.Sets.Sum(s => s.Reps * s.Weight);

                if (session.SessionDate > summary.LastPerformedDate)
                {
                    summary.LastPerformedDate = session.SessionDate;
                }
            }
        }

        return new HistorySummary
        {
            Exercises = byExercise.Values.OrderBy(v => v.ExerciseName).ToList()
        };
    }
}
