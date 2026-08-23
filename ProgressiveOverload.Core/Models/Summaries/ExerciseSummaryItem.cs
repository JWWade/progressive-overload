namespace ProgressiveOverload.Core.Models.Summaries;

public class ExerciseSummaryItem
{
    public string ExerciseName { get; set; } = string.Empty;
    public int TotalSets { get; set; }
    public int TotalReps { get; set; }
    public decimal TotalVolume { get; set; }
    public DateOnly LastPerformedDate { get; set; }
}
