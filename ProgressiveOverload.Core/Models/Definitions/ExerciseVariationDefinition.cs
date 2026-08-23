namespace ProgressiveOverload.Core.Models.Definitions;

public class ExerciseVariationDefinition
{
    public string Name { get; set; } = string.Empty;
    public decimal VolumeMultiplier { get; set; } = 1m;
    public string? Notes { get; set; }
}
