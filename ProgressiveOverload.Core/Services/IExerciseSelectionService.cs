using ProgressiveOverload.Core.Models.Recommendations;

namespace ProgressiveOverload.Core.Services;

public interface IExerciseSelectionService
{
    Task<VariationRecommendation?> RecommendNextVariationAsync(
        string categoryName,
        DateOnly? asOfDate = null,
        CancellationToken cancellationToken = default);
}
