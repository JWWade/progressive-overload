namespace ProgressiveOverload.Core.Models;

public class ExerciseEntry
{
    public string ExerciseName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<SetEntry> Sets { get; set; } = [];
}
