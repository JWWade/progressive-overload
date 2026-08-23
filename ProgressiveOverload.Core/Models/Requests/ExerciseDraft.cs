namespace ProgressiveOverload.Core.Models.Requests;

public class ExerciseDraft
{
    public string ExerciseName { get; set; } = string.Empty;
    public string? VariationName { get; set; }
    public string? Notes { get; set; }
    public List<SetDraft> Sets { get; set; } = [];
}
