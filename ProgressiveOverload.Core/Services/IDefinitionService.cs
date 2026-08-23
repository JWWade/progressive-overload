using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Models.Definitions;
using ProgressiveOverload.Core.Models.Requests;

namespace ProgressiveOverload.Core.Services;

public interface IDefinitionService
{
    Task<ExerciseCatalog> GetCatalogAsync(CancellationToken cancellationToken = default);

    Task<OperationResult> UpsertExerciseDefinitionAsync(
        UpsertExerciseDefinitionRequest request,
        CancellationToken cancellationToken = default);
}
