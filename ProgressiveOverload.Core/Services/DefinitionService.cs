using ProgressiveOverload.Core.Models;
using ProgressiveOverload.Core.Models.Definitions;
using ProgressiveOverload.Core.Models.Requests;
using ProgressiveOverload.Core.Persistence;

namespace ProgressiveOverload.Core.Services;

public class DefinitionService : IDefinitionService
{
    private readonly IProgressiveOverloadRepository _repository;

    public DefinitionService(IProgressiveOverloadRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExerciseCatalog> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var data = await _repository.LoadAsync(cancellationToken);
        data.ExerciseCatalog ??= new ExerciseCatalog();
        return data.ExerciseCatalog;
    }

    public async Task<OperationResult> UpsertExerciseDefinitionAsync(
        UpsertExerciseDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
        {
            return OperationResult.Failure("Category name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ExerciseName))
        {
            return OperationResult.Failure("Exercise name is required.");
        }

        if (request.Variations.Count == 0)
        {
            return OperationResult.Failure("At least one variation is required.");
        }

        if (request.Variations.Count > 2)
        {
            return OperationResult.Failure("A maximum of two variations is supported.");
        }

        foreach (var variation in request.Variations)
        {
            if (string.IsNullOrWhiteSpace(variation.Name))
            {
                return OperationResult.Failure("Each variation must include a name.");
            }

            if (variation.VolumeMultiplier <= 0)
            {
                return OperationResult.Failure($"Volume multiplier must be greater than zero for {variation.Name}.");
            }
        }

        var distinctVariationCount = request.Variations
            .Select(v => v.Name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        if (distinctVariationCount != request.Variations.Count)
        {
            return OperationResult.Failure("Variation names must be unique per exercise.");
        }

        var data = await _repository.LoadAsync(cancellationToken);
        data.ExerciseCatalog ??= new ExerciseCatalog();

        var categoryName = request.CategoryName.Trim();
        if (!data.ExerciseCatalog.Categories.Any(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase)))
        {
            data.ExerciseCatalog.Categories.Add(new MovementCategory { Name = categoryName });
        }

        var exerciseName = request.ExerciseName.Trim();
        var existing = data.ExerciseCatalog.Exercises
            .FirstOrDefault(e => string.Equals(e.Name, exerciseName, StringComparison.OrdinalIgnoreCase));

        var mappedVariations = request.Variations
            .Select(v => new ExerciseVariationDefinition
            {
                Name = v.Name.Trim(),
                VolumeMultiplier = v.VolumeMultiplier,
                Notes = string.IsNullOrWhiteSpace(v.Notes) ? null : v.Notes.Trim()
            })
            .ToList();

        if (existing is null)
        {
            data.ExerciseCatalog.Exercises.Add(new ExerciseDefinition
            {
                Name = exerciseName,
                CategoryName = categoryName,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                Variations = mappedVariations
            });
        }
        else
        {
            existing.CategoryName = categoryName;
            existing.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            existing.Variations = mappedVariations;
        }

        data.DataVersion = Math.Max(data.DataVersion, 2);
        await _repository.SaveAsync(data, cancellationToken);
        return OperationResult.Success();
    }
}
