using ProgressiveOverload.Core.Models.Recommendations;
using ProgressiveOverload.Core.Persistence;

namespace ProgressiveOverload.Core.Services;

public class ExerciseSelectionService : IExerciseSelectionService
{
    private readonly IProgressiveOverloadRepository _repository;

    public ExerciseSelectionService(IProgressiveOverloadRepository repository)
    {
        _repository = repository;
    }

    public async Task<VariationRecommendation?> RecommendNextVariationAsync(
        string categoryName,
        DateOnly? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return null;
        }

        var data = await _repository.LoadAsync(cancellationToken);
        var effectiveDate = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);
        var category = categoryName.Trim();

        var exercises = data.ExerciseCatalog.Exercises
            .Where(e => string.Equals(e.CategoryName, category, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (exercises.Count == 0)
        {
            return null;
        }

        VariationRecommendation? best = null;

        foreach (var exercise in exercises)
        {
            for (var variationIndex = 0; variationIndex < exercise.Variations.Count; variationIndex++)
            {
                var variation = exercise.Variations[variationIndex];
                var matchingSessions = data.WorkoutSessions
                    .SelectMany(s => s.Exercises.Select(e => new { SessionDate = s.SessionDate, Exercise = e }))
                    .Where(x => string.Equals(x.Exercise.ExerciseName, exercise.Name, StringComparison.OrdinalIgnoreCase))
                    .Where(x => MatchesVariation(x.Exercise.VariationName, variation.Name, variationIndex))
                    .ToList();

                var weightedVolume = matchingSessions
                    .Sum(x => x.Exercise.Sets.Sum(set => set.Reps * set.Weight)) * variation.VolumeMultiplier;

                var lastPerformed = matchingSessions
                    .Select(x => (DateOnly?)x.SessionDate)
                    .OrderByDescending(x => x)
                    .FirstOrDefault();

                var daysSince = lastPerformed.HasValue
                    ? effectiveDate.DayNumber - lastPerformed.Value.DayNumber
                    : 3650;

                var recencyPenalty = Math.Max(0, 21 - daysSince) * 1000m;
                var score = weightedVolume + recencyPenalty;

                var recommendation = new VariationRecommendation
                {
                    CategoryName = exercise.CategoryName,
                    ExerciseName = exercise.Name,
                    VariationName = variation.Name,
                    WeightedVolume = decimal.Round(weightedVolume, 2),
                    LastPerformedDate = lastPerformed,
                    DaysSinceLastPerformed = daysSince,
                    Score = decimal.Round(score, 2),
                    Reason = BuildReason(weightedVolume, lastPerformed, daysSince)
                };

                if (best is null || recommendation.Score < best.Score)
                {
                    best = recommendation;
                }
            }
        }

        return best;
    }

    private static bool MatchesVariation(string? loggedVariation, string candidateVariation, int variationIndex)
    {
        if (!string.IsNullOrWhiteSpace(loggedVariation))
        {
            return string.Equals(loggedVariation.Trim(), candidateVariation, StringComparison.OrdinalIgnoreCase);
        }

        // Backward compatibility for legacy logs without variation info.
        return variationIndex == 0;
    }

    private static string BuildReason(decimal weightedVolume, DateOnly? lastPerformed, int daysSince)
    {
        if (!lastPerformed.HasValue)
        {
            return "Never performed yet, so it is prioritized.";
        }

        return $"Weighted volume={decimal.Round(weightedVolume, 2)} and last performed {daysSince} day(s) ago.";
    }
}
