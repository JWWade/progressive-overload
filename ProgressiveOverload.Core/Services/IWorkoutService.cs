using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Models.Requests;

namespace ProgressiveOverload.Core.Services;

public interface IWorkoutService
{
    Task<OperationResult> LogWorkoutAsync(WorkoutSessionDraft request, CancellationToken cancellationToken = default);
}
