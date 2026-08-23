using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Models.Requests;

namespace ProgressiveOverload.Core.Services;

public interface IProfileService
{
    Task<OperationResult> UpsertProfileAsync(ProfileUpsertRequest request, CancellationToken cancellationToken = default);
}
