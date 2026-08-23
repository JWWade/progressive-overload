namespace ProgressiveOverload.Core.Models;

public class ExerciseBaseline
{
    public string ExerciseName { get; set; } = string.Empty;
    public decimal BaselineWeight { get; set; }
    public int BaselineReps { get; set; }
    public string? Notes { get; set; }
}
