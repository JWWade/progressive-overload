namespace ProgressiveOverload.Core.Models.Requests;

public class WorkoutSessionDraft
{
    public DateOnly SessionDate { get; set; }
    public string? Notes { get; set; }
    public List<ExerciseDraft> Exercises { get; set; } = [];
}
