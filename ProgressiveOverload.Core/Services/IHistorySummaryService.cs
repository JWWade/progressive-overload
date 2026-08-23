using ProgressiveOverload.Core.Models.Summaries;

namespace ProgressiveOverload.Core.Services;

public interface IHistorySummaryService
{
    Task<HistorySummary> GetSummaryAsync(CancellationToken cancellationToken = default);
}
