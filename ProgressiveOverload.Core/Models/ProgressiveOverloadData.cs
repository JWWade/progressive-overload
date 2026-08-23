namespace ProgressiveOverload.Core.Models;

public class ProgressiveOverloadData
{
    public int DataVersion { get; set; } = 1;
    public UserProfile? UserProfile { get; set; }
    public List<WorkoutSession> WorkoutSessions { get; set; } = [];
}
