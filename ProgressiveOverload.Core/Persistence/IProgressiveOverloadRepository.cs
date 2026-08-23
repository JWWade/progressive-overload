using ProgressiveOverload.Core.Models;

namespace ProgressiveOverload.Core.Persistence;

public interface IProgressiveOverloadRepository
{
    string DataFilePath { get; }

    Task<ProgressiveOverloadData> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(ProgressiveOverloadData data, CancellationToken cancellationToken = default);
}
