namespace ProgressiveOverload.Core.Models.Definitions;

public class ExerciseDefinition
{
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<ExerciseVariationDefinition> Variations { get; set; } = [];
}
