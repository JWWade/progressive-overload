namespace ProgressiveOverload.Core.Models.Definitions;

public class ExerciseCatalog
{
    public List<MovementCategory> Categories { get; set; } = [];
    public List<ExerciseDefinition> Exercises { get; set; } = [];
}
