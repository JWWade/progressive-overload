using ProgressiveOverload.Core.Models.Definitions;

namespace ProgressiveOverload.Core.Models;

public class ProgressiveOverloadData
{
    public int DataVersion { get; set; } = 2;
    public UserProfile? UserProfile { get; set; }
    public List<WorkoutSession> WorkoutSessions { get; set; } = [];
    public ExerciseCatalog ExerciseCatalog { get; set; } = new();
}
