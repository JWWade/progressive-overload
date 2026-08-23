namespace ProgressiveOverload.Core.Models.Recommendations;

public class VariationRecommendation
{
    public string CategoryName { get; set; } = string.Empty;
    public string ExerciseName { get; set; } = string.Empty;
    public string VariationName { get; set; } = string.Empty;
    public decimal WeightedVolume { get; set; }
    public DateOnly? LastPerformedDate { get; set; }
    public int DaysSinceLastPerformed { get; set; }
    public decimal Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}
