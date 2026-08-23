namespace ProgressiveOverload.Core.Models;

public class WorkoutSession
{
    public Guid Id { get; set; }
    public DateOnly SessionDate { get; set; }
    public string? Notes { get; set; }
    public List<ExerciseEntry> Exercises { get; set; } = [];
}
