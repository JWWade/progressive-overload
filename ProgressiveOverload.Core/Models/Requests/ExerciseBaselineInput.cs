namespace ProgressiveOverload.Core.Models.Requests;

public class ExerciseBaselineInput
{
    public string ExerciseName { get; set; } = string.Empty;
    public decimal BaselineWeight { get; set; }
    public int BaselineReps { get; set; }
    public string? Notes { get; set; }
}
