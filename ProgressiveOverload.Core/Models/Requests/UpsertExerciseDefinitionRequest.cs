namespace ProgressiveOverload.Core.Models.Requests;

public class UpsertExerciseDefinitionRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string ExerciseName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<ExerciseVariationInput> Variations { get; set; } = [];
}
