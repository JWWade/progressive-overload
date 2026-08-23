namespace ProgressiveOverload.Core.Models;

public class UserProfile
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<ExerciseBaseline> ExerciseBaselines { get; set; } = [];
}
