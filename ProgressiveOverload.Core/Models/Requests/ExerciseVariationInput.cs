namespace ProgressiveOverload.Core.Models.Requests;

public class ExerciseVariationInput
{
    public string Name { get; set; } = string.Empty;
    public decimal VolumeMultiplier { get; set; } = 1m;
    public string? Notes { get; set; }
}
