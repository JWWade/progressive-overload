namespace ProgressiveOverload.Core.Models.Requests;

public class ProfileUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public List<ExerciseBaselineInput> ExerciseBaselines { get; set; } = [];
}
