using System.Text.Json;
using ProgressiveOverload.Core.Models;

namespace ProgressiveOverload.Core.Persistence;

public class JsonFileProgressiveOverloadRepository : IProgressiveOverloadRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonFileProgressiveOverloadRepository(string? appFolderName = null, string? fileName = null)
    {
        var folder = appFolderName ?? "ProgressiveOverload";
        var dataFileName = fileName ?? "progressive-overload-data.json";
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        DataFilePath = Path.Combine(root, folder, dataFileName);
    }

    public string DataFilePath { get; }

    public async Task<ProgressiveOverloadData> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(DataFilePath))
        {
            return new ProgressiveOverloadData();
        }

        await using var stream = File.OpenRead(DataFilePath);

        try
        {
            var data = await JsonSerializer.DeserializeAsync<ProgressiveOverloadData>(stream, SerializerOptions, cancellationToken);
            return data ?? new ProgressiveOverloadData();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse persisted data file: {DataFilePath}", ex);
        }
    }

    public async Task SaveAsync(ProgressiveOverloadData data, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(DataFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(DataFilePath);
        await JsonSerializer.SerializeAsync(stream, data, SerializerOptions, cancellationToken);
    }
}
